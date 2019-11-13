using UnityEngine;
using UnityEngine.Rendering;

public class AddASphereTutorial : RayTracingTutorial
{
    public AddASphereTutorial(RayTracingTutorialAsset vAsset) : base(vAsset)
    { }

    public override void Render(ScriptableRenderContext vContext, Camera vCamera)
    {
        base.Render(vContext, vCamera);

        var RenderTarget = RequireRenderTarget(vCamera);
        var AccelerationStructure = _rayTracingRenderPipeline.RequestAccelerationStructure();

        var Command = CommandBufferPool.Get(typeof(AddASphereTutorial).Name);
        try
        {
            using (new ProfilingSample(Command, "RayTracing"))
            {
                Command.SetRayTracingShaderPass(_rayTracingShader, "RayTracing");
                Command.SetRayTracingAccelerationStructure(_rayTracingShader, _rayTracingRenderPipeline._accelerationStructureShaderId, AccelerationStructure);
                Command.SetRayTracingTextureParam(_rayTracingShader, _renderTargetShaderId, RenderTarget);
                Command.DispatchRays(_rayTracingShader, "AddASphereRaygenShader", (uint)RenderTarget.rt.width, (uint)RenderTarget.rt.height, 1, vCamera);
            }
            vContext.ExecuteCommandBuffer(Command);

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
