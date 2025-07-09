using System;
using DG.Tweening;
using UnityEngine;

namespace Content.Scripts.Menu.UI
{
    public abstract class WindowBase : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;

        private bool isOpened = false;
        
        public event Action<WindowBase> OnShow;

        public bool IsOpened => isOpened;

        public virtual void Show()
        {
            canvas.enabled = true;
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, 0.25f).SetLink(gameObject);
            
            OnShow?.Invoke(this);

            isOpened = true;
        }


        public void Hide(bool instantly = false)
        {
            if (!instantly)
            {
                canvasGroup.DOFade(0, 0.25f).SetLink(gameObject).onComplete += delegate
                {
                    canvas.enabled = false;
                };
            }
            else
            {
                canvas.enabled = false;
            }
            isOpened = false;
        }
    }
}