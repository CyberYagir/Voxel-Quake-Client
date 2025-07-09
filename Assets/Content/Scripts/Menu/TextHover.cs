using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Content.Scripts.Menu
{
    public class TextHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private float power = 1.5f;
        [SerializeField] private Color startColor;

        [Inject]
        private void Construct()
        {
            if (text)
            {
                startColor = text.color;
            }
        }

        private void OnValidate()
        {
            if (text == null)
            {
                text = GetComponent<TMP_Text>();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (text)
            {
                text.DOColor(startColor * power, 0.25f).SetLink(gameObject);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (text)
            {
                text.DOColor(startColor, 0.25f).SetLink(gameObject);
            }
        }
    }
}
