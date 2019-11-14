using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public abstract class RayTracingTutorial
{
    private static class CameraShaderParams
    {
        public static readonly int _worldSpaceCameraPosShaderId = Shader.PropertyToID("_WorldSpaceCameraPos");
        public static readonly int _invCameraViewProjShaderId = Shader.PropertyToID("_InvCameraViewProj");
        public static readonly int _cameraFarDistanceShaderId = Shader.PropertyToID("_CameraFarDistance");
    }
    private RayTracingTutorialAsset _rayTracingTutorialAsset;
    private readonly Dictionary<int, RTHandle> _renderTargets = new Dictionary<int, RTHandle>();
    private readonly Dictionary<int, Vector4> _renderTargetSizes = new Dictionary<int, Vector4>();

    protected RayTracingShader _rayTracingShader;
    protected RayTracingRenderPipeline _rayTracingRenderPipeline;
    protected readonly int _renderTargetShaderId = Shader.PropertyToID("_RenderTarget");
    protected readonly int _renderTargetSizeShaderId = Shader.PropertyToID("_RenderTargetSize");

    protected RayTracingTutorial(RayTracingTutorialAsset vAsset)
    {
        _rayTracingTutorialAsset = vAsset;
    }
    public virtual bool Init(RayTracingRenderPipeline vPipeline)
    {
        _rayTracingRenderPipeline = vPipeline;
        _rayTracingShader = _rayTracingTutorialAsset._rayTracingShader;
        return true;
    }
    public virtual void Render(ScriptableRenderContext vContext, Camera vCamera)
    {
        SetCamera(vCamera);
    }
    public virtual void Dispose(bool vIsDisposing)
    {
        foreach(var TempTarget in _renderTargets)
            RTHandles.Release(TempTarget.Value);

        _renderTargets.Clear();
    }
    protected RTHandle RequireRenderTarget(Camera vCamera)
    {
        var CameraId = vCamera.GetInstanceID();

        if (_renderTargets.TryGetValue(CameraId, out var OutRenderTarget))
            return OutRenderTarget;

        OutRenderTarget = RTHandles.Alloc(
            vCamera.pixelWidth, 
            vCamera.pixelHeight, 
            1, 
            DepthBits.None, 
            GraphicsFormat.R32G32B32A32_SFloat, 
            FilterMode.Point, 
            TextureWrapMode.Clamp, 
            TextureDimension.Tex2D, 
            true, 
            false, 
            false, 
            false, 
            1, 
            0f, 
            MSAASamples.None,
            false,
            false,
            RenderTextureMemoryless.None, 
            $"RenderTarget_{vCamera.name}");

        _renderTargets.Add(CameraId, OutRenderTarget);

        return OutRenderTarget;
    }
    protected Vector4 RequireRenderTargetSize(Camera vCamera)
    {
        var CameraId = vCamera.GetInstanceID();

        if (_renderTargetSizes.TryGetValue(CameraId, out var OutRenderTargetSize))
            return OutRenderTargetSize;

        OutRenderTargetSize = new Vector4(
            vCamera.pixelWidth, 
            vCamera.pixelHeight, 
            1.0f / (float)vCamera.pixelWidth, 
            1.0f / (float)vCamera.scaledPixelHeight);
        
        _renderTargetSizes.Add(CameraId, OutRenderTargetSize);

        return OutRenderTargetSize;
    }
    private static void SetCamera(Camera vCamera)
    {
        Shader.SetGlobalVector(CameraShaderParams._worldSpaceCameraPosShaderId, vCamera.transform.position);

        var ProjMat = GL.GetGPUProjectionMatrix(vCamera.projectionMatrix, false);
        var ViewMat = vCamera.worldToCameraMatrix;
        var VPMat = ProjMat * ViewMat;
        var InvVPMat = Matrix4x4.Inverse(VPMat);

        Shader.SetGlobalMatrix(CameraShaderParams._invCameraViewProjShaderId, InvVPMat);
        Shader.SetGlobalFloat(CameraShaderParams._cameraFarDistanceShaderId, vCamera.farClipPlane);
    }
}
