using UnityEngine;

[CreateAssetMenu(fileName = "OutputColorTutorialAsset", menuName = "RayTracingRendering/OutputColorTutorialAsset")]
public class OutputColorTutorialAsset : RayTracingTutorialAsset
{
    public override RayTracingTutorial CreateTutorial()
    {
        return new OutputColorTutorial(this); 
    }
}
