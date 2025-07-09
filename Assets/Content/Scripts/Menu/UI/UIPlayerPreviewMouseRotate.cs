using Content.Scripts.Menu.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Content.Scripts.Menu.UI
{
    public class UIPlayerPreviewMouseRotate : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private PlayerRenderService playerService;

        [Inject]
        private void Construct(PlayerRenderService playerService)
        {
            this.playerService = playerService;
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            playerService.IsCanRotate(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            playerService.IsCanRotate(false);
        }
    }
}
