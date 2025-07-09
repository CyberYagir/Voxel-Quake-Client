using Content.Scripts.Services;
using Content.Scripts.Services.Net;
using DG.Tweening;
using UnityEngine;

namespace Content.Scripts.Menu.UI
{
    public class UIServerConnectLoader : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        public void Init(NetService netService)
        {
            netService.OnPlayerConnectionStart += NetServiceOnOnPlayerConnected;
            netService.OnPlayerDisconnected += NetServiceOnOnPlayerDisconnected;
        }

        private void NetServiceOnOnPlayerDisconnected()
        {
            canvasGroup.DOFade(0, 0.25f).onComplete += () =>
            {
                canvas.enabled = false;
            };
        }

        private void NetServiceOnOnPlayerConnected()
        {
            canvas.enabled = true;
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1f, 0.25f);
        }
        
        
    }
}
