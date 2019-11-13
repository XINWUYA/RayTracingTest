using UnityEngine;

[CreateAssetMenu(fileName = "AddASphereTutorialAsset", menuName = "RayTracingRendering/AddASphereTutorialAsset")]
public class AddASphereTutorialAsset : RayTracingTutorialAsset
{
    public override RayTracingTutorial CreateTutorial()
    {
        return new AddASphereTutorial(this);
    }
}
