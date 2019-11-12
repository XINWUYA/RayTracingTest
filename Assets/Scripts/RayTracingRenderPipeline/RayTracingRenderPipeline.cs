using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class RayTracingRenderPipeline : RenderPipeline
{
    private RayTracingRenderPipelineAsset _rayTracingRenderPipelineAsset;
    private RayTracingAccelerationStructure _rayTracingAccelerationStructure;
    private RayTracingTutorial _rayTracingTutorial;
    private readonly Dictionary<int, ComputeBuffer> _PRNGStates = new Dictionary<int, ComputeBuffer>();

    public readonly int _accelerationStructureShaderId = Shader.PropertyToID("_AccelerationStructure");

    public RayTracingRenderPipeline(RayTracingRenderPipelineAsset vAsset)
    {
        _rayTracingRenderPipelineAsset = vAsset;
        _rayTracingAccelerationStructure = new RayTracingAccelerationStructure();
        _rayTracingTutorial = _rayTracingRenderPipelineAsset._rayTracingTutorialAsset.CreateTutorial();
        
        if(null == _rayTracingTutorial)
        {
            Debug.LogError("Can't create tutorial.");
            return;
        }

        if(!_rayTracingTutorial.Init(this))
        {
            _rayTracingTutorial = null;
            Debug.LogError("Initialize tutorial failed.");
            return;
        }
    }

    public RayTracingAccelerationStructure RequestAccelerationStructure()
    {
        return _rayTracingAccelerationStructure;
    }

    //require a PRNG compute buffer for camera.
    public ComputeBuffer RequirePRNGStates(Camera vCamera)
    {
        var CameraId = vCamera.GetInstanceID();

        if (_PRNGStates.TryGetValue(CameraId, out var OutBuffer))
            return OutBuffer;

        OutBuffer = new ComputeBuffer(vCamera.pixelWidth * vCamera.pixelHeight, 4 * 4, ComputeBufferType.Structured, ComputeBufferMode.Immutable);

        var Mt19937 = new MersenneTwister.MT.mt19937ar_cok_opt_t();
        Mt19937.init_genrand((uint)System.DateTime.Now.Ticks);

        var Data = new uint[vCamera.pixelWidth * vCamera.pixelHeight * 4];
        for (var i = 0; i < vCamera.pixelWidth * vCamera.pixelHeight * 4; ++i)
            Data[i] = Mt19937.genrand_int32();
        OutBuffer.SetData(Data);

        _PRNGStates.Add(CameraId, OutBuffer);
        return OutBuffer;
    }

    protected override void Render(ScriptableRenderContext vContext, Camera[] vCameras)
    {
        if(!SystemInfo.supportsRayTracing)
        {
            Debug.LogError("Your system is not support ray tracing. Please check your graphic API is D3D12 and os is Windows 10.");
            return;
        }

        BeginFrameRendering(vContext, vCameras);

        System.Array.Sort(vCameras, (lhs, rhs) => (int)(lhs.depth - rhs.depth));

        BuildAccelerationStructure();

        foreach (var TempCamera in vCameras)
        {
            if (TempCamera.cameraType != CameraType.Game && TempCamera.cameraType != CameraType.SceneView)
                continue;

            BeginCameraRendering(vContext, TempCamera);
            _rayTracingTutorial?.Render(vContext, TempCamera);
            vContext.Submit();
            EndCameraRendering(vContext, TempCamera);
        }

        EndFrameRendering(vContext, vCameras);
    }

    protected override void Dispose(bool vIsDisposing)
    {
        if(null != _rayTracingTutorial)
        {
            _rayTracingTutorial.Dispose(vIsDisposing);
            _rayTracingTutorial = null;
        }

        foreach (var TempPair in _PRNGStates)
        {
            TempPair.Value.Release();
        }
        _PRNGStates.Clear();

        if(null != _rayTracingAccelerationStructure)
        {
            _rayTracingAccelerationStructure.Dispose();
            _rayTracingAccelerationStructure = null;
        }
    }

    //build the ray tracing acceleration structure.
    private void BuildAccelerationStructure()
    {
        if (null == SceneManager.Instance || !SceneManager.Instance._isDirty)
            return;

        _rayTracingAccelerationStructure.Dispose();
        _rayTracingAccelerationStructure = new RayTracingAccelerationStructure();
        SceneManager.Instance.FillAccelerationStructure(ref _rayTracingAccelerationStructure);

        _rayTracingAccelerationStructure.Build();

        SceneManager.Instance._isDirty = false;
    }
}
