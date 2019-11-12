using UnityEngine;
using UnityEngine.Experimental.Rendering;

public abstract class RayTracingTutorialAsset : ScriptableObject
{
    public RayTracingShader _rayTracingShader;
    public abstract RayTracingTutorial CreateTutorial();
}
