using System;
using System.Collections;
using Content.Scripts.Game.Services;
using Content.Scripts.Scriptable;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Menu.Services
{
    public class PlayerRenderService : MonoBehaviour
    {
        [SerializeField] private Material playerMaterial;
        [SerializeField] private Transform rotator;
        [SerializeField] private float rotateSpeed;
        private PlayerConfigObject config;
        private bool isCanRotate;

        [Inject]
        private void Construct(PlayerConfigObject config)
        {
            this.config = config;
            this.config.OnConfigChanged.AddListener(UpdateColor);
            UpdateColor();
        }

        public void IsCanRotate(bool state)
        {
            isCanRotate = state;
            if (state)
            {
                StartCoroutine(Loop());
            }
        }

        IEnumerator Loop()
        {
            while (InputService.IsShootPressed)
            {
                // Если MouseX уже учитывает deltaTime, убираем его
                float normalizedInput = InputService.MouseX / Time.deltaTime;
                rotator.Rotate(Vector3.up * normalizedInput * Time.deltaTime * rotateSpeed);
                yield return null;
            }
        }

   

        private void UpdateColor()
        {
            if (ColorUtility.TryParseHtmlString(config.PlayerColor, out var color))
            {
                playerMaterial.SetColor("_BaseColor", color);
            }
        }
    }
}
