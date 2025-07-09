using Content.Scripts.Scriptable;
using UnityEngine;

namespace Content.Scripts.Menu.UI
{
    public class UIOptionsWindow : WindowBase
    {
        [SerializeField] private UIOptionsSlider gameFPS;
        [SerializeField] private UIOptionsSlider menuFPS;
        [SerializeField] private UIOptionsSlider mouseSensitivity;
        [SerializeField] private UIOptionsSlider fov;
        [SerializeField] private UIOptionsString nickname;
        [SerializeField] private UIOptionsString color;
        [SerializeField] private UITabs tabs;
        private PlayerConfigObject playerConfigObject;


        public void Init(PlayerConfigObject playerConfigObject)
        {
            this.playerConfigObject = playerConfigObject;
            gameFPS.Init(playerConfigObject.GameFps, 30, 265, OnGameFpsChange);
            menuFPS.Init(playerConfigObject.MenuFps, 30, 265, OnMenuFpsChange);
            mouseSensitivity.Init(playerConfigObject.Sens, 0, 10, OnMouseSensitivityChange);
            fov.Init(playerConfigObject.FOV, 30, 120, OnFovChange);
            nickname.Init(playerConfigObject.PlayerName, OnPlayerNameChange);
            color.Init(playerConfigObject.PlayerColor, OnPlayerColorChange);
            tabs.Init();
        }

        private void OnPlayerColorChange(string value)
        {
            playerConfigObject.SetColor(value);
            playerConfigObject.Save();  
        }

        private void OnPlayerNameChange(string value)
        {
            playerConfigObject.SetNickname(value);
            playerConfigObject.Save();         
        }

        private void OnFovChange(float value)
        {
            playerConfigObject.SetFov((int)value);
            playerConfigObject.Save();         
        }

        private void OnMouseSensitivityChange(float value)
        {
            playerConfigObject.SetSens(value);
            playerConfigObject.Save();
        }

        private void OnMenuFpsChange(float value)
        {
            playerConfigObject.SetMenuFPS((int)value);
            Application.targetFrameRate = (int)value;
            playerConfigObject.Save();
        }

        private void OnGameFpsChange(float value)
        {
            playerConfigObject.SetGameFPS((int)value);
            playerConfigObject.Save();
        }
    }
}
