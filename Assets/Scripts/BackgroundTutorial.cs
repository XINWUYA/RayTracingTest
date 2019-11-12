using UnityEngine;
using UnityEngine.Rendering;

public class BackgroundTutorial : RayTracingTutorial
{
    public BackgroundTutorial(RayTracingTutorialAsset vAsset) : base(vAsset)
    { }

    public override void Render(ScriptableRenderContext vContext, Camera vCamera)
    {
        base.Render(vContext, vCamera);

        var RenderTarget = RequireRenderTarget(vCamera);

        var Command = CommandBufferPool.Get(typeof(BackgroundTutorial).Name);
        try
        {
            using (new ProfilingSample(Command, "RayTracing"))
            {
                Command.SetRayTracingTextureParam(_rayTracingShader, _renderTargetShaderId, RenderTarget);
                Command.DispatchRays(_rayTracingShader, "BackgroundRaygenShader", (uint)RenderTarget.rt.width, (uint)RenderTarget.rt.height, 1, vCamera);
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
