using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "RayTracingRenderPipelineAsset", menuName = "RayTracingRendering/RayTracingRenderPipelineAsset")]
public class RayTracingRenderPipelineAsset : RenderPipelineAsset
{
    public RayTracingTutorialAsset _rayTracingTutorialAsset;

    protected override RenderPipeline CreatePipeline()
    {
        return new RayTracingRenderPipeline(this);
    }
}
