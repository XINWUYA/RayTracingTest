using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class TutorialMonoBehaviour : MonoBehaviour
{
    public RenderPipelineAsset _currentRenderPipelineAsset;
    private RenderPipelineAsset _oldRenderPipelineAsset;

    public IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        _oldRenderPipelineAsset = GraphicsSettings.renderPipelineAsset;
        GraphicsSettings.renderPipelineAsset = _currentRenderPipelineAsset;
    }

    public void OnDestroy()
    {
        GraphicsSettings.renderPipelineAsset = _oldRenderPipelineAsset;
    }
}
