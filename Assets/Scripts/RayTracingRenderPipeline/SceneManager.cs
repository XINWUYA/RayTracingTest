using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class SceneManager : MonoBehaviour
{
    const int MAX_NUM_SUB_MESHES = 32;
    private bool[] _subMeshFlagArray = new bool[MAX_NUM_SUB_MESHES];
    private bool[] _subMeshCutoffArray = new bool[MAX_NUM_SUB_MESHES];
    private static SceneManager _sceneManagerInstance;

    public Renderer[] _renderers;//all renderers.

    [System.NonSerialized]
    public bool _isDirty = true;//whether need to rebuild acceleration structure.

    public static SceneManager Instance //get instance.
    {
        get
        {
            if (_sceneManagerInstance != null)
                return _sceneManagerInstance;

            _sceneManagerInstance = GameObject.FindObjectOfType<SceneManager>();
            _sceneManagerInstance?.Init();
            return _sceneManagerInstance;
        }
    }

    public void Awake()
    {
        if (Application.isPlaying)
            DontDestroyOnLoad(this);

        _isDirty = true;
    }

    public void FillAccelerationStructure(ref RayTracingAccelerationStructure vRayTracingAccelerationStructure)
    {
        foreach(var r in _renderers)
        {
            if (r)
                vRayTracingAccelerationStructure.AddInstance(r, _subMeshFlagArray, _subMeshCutoffArray);
        }
    }

    private void Init()
    {
        for(var i = 0; i < MAX_NUM_SUB_MESHES; ++i)
        {
            _subMeshFlagArray[i] = true;
            _subMeshCutoffArray[i] = false;
        }
    }
}
