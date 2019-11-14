using UnityEngine;

[CreateAssetMenu(fileName = "AntiAliasingTutorialAsset", menuName = "RayTracingRendering/AntiAliasingTutorialAsset")]
public class AntiAliasingTutorialAsset : RayTracingTutorialAsset
{
    public override RayTracingTutorial CreateTutorial()
    {
        return new AntiAliasingTutorial(this);
    }
}
