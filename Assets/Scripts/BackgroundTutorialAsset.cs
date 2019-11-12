using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundTutorialAsset", menuName = "RayTracingRendering/BackgroundTutorialAsset")]
public class BackgroundTutorialAsset : RayTracingTutorialAsset
{
    public override RayTracingTutorial CreateTutorial()
    {
        return new BackgroundTutorial(this);
    }
}
