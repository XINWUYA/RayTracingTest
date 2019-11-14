using UnityEngine;
using UnityEngine.Rendering;

public class AntiAliasingTutorial : RayTracingTutorial
{
    private readonly int _PRNGStatesShaderId = Shader.PropertyToID("_PRNGStates");
    private readonly int _frameIndexShaderId = Shader.PropertyToID("_FrameIndex");
    private int _frameIndex = 0;
    public AntiAliasingTutorial(RayTracingTutorialAsset vAsset) : base(vAsset)
    { }

    public override void Render(ScriptableRenderContext vContext, Camera vCamera)
    {
        base.Render(vContext, vCamera);

        var RenderTarget = RequireRenderTarget(vCamera);
        var RenderTargetSize = RequireRenderTargetSize(vCamera);
        var AccelerationStructure = _rayTracingRenderPipeline.RequestAccelerationStructure();
        var PRNGStates = _rayTracingRenderPipeline.RequirePRNGStates(vCamera);

        var Command = CommandBufferPool.Get(typeof(AntiAliasingTutorial).Name);
        try
        {
            if(_frameIndex < 1000)
            {
                using (new ProfilingSample(Command, "RayTracing"))
                {
                    Command.SetRayTracingShaderPass(_rayTracingShader, "RayTracing");
                    Command.SetRayTracingAccelerationStructure(_rayTracingShader, _rayTracingRenderPipeline._accelerationStructureShaderId, AccelerationStructure);
                    Command.SetRayTracingIntParam(_rayTracingShader, _frameIndexShaderId, _frameIndex);
                    Command.SetRayTracingBufferParam(_rayTracingShader, _PRNGStatesShaderId, PRNGStates);
                    Command.SetRayTracingTextureParam(_rayTracingShader, _renderTargetShaderId, RenderTarget);
                    Command.SetRayTracingVectorParam(_rayTracingShader, _renderTargetSizeShaderId, RenderTargetSize);
                    Command.DispatchRays(_rayTracingShader, "AntiAliasingRaygenShader", (uint)RenderTarget.rt.width, (uint)RenderTarget.rt.height, 1, vCamera);
                }
                vContext.ExecuteCommandBuffer(Command);

                if (vCamera.cameraType == CameraType.Game)
                    _frameIndex++;
            }

            using (new ProfilingSample(Command, "FinalBlit"))
            {
                Command.Blit(RenderTarget, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
            }
            vContext.ExecuteCommandBuffer(Command);
        }
        finally
        {
            CommandBufferPool.Release(Command);
        }
    }
}
