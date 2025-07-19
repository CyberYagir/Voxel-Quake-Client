using Content.Scripts.Services;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace Content.Scripts.Boot
{
    public class Boot : MonoBehaviour
    {
        [Inject]
        private void Construct(ScenesService scenesService)
        {
            scenesService.LoadScene(ScenesService.EScene.Menu);
            
            Debug.Log(GraphicsSettings.useScriptableRenderPipelineBatching);
        }
    }
}