using System;
using Content.Scripts.Scriptable;
using Content.Scripts.Services;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Menu.Services
{
    public class MenuLoaderService : MonoBehaviour
    {
        private PlayerConfigObject playerConfigObject;

        [Inject]
        private void Construct(PlayerConfigObject playerConfigObject)
        {
            this.playerConfigObject = playerConfigObject;
        }

        private void Start()
        {
            Application.targetFrameRate = playerConfigObject.MenuFps;
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
