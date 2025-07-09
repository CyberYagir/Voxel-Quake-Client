using System.Collections.Generic;
using Content.Scripts.Menu.UI;
using Content.Scripts.Scriptable;
using Content.Scripts.Services;
using Content.Scripts.Services.Net;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Content.Scripts.Menu.Services
{
    public class MenuUIService : MonoBehaviour
    {
        [System.Serializable]
        public class WindowsManager
        {
            [SerializeField] private List<WindowBase> windows = new List<WindowBase>();


            public void AddWindows(params WindowBase[] window)
            {
                windows.Add(window);

                foreach (var w in window)
                {
                    w.Hide(true);
                }
            }


            public void OpenWindow(WindowBase window)
            {
             
                
                for (var i = 0; i < windows.Count; i++)
                {
                    if (windows[i].IsOpened && windows[i] != window)
                    {
                        windows[i].Hide();
                    }
                }
                
                if (window.IsOpened)
                {
                    window.Hide();
                }
                else
                {
                    window.Show();
                }
            }
        }

        [SerializeField] private WindowsManager windowsManager;
        [SerializeField] private UIServersBrowserWindow uiServersBrowserWindow;
        [SerializeField] private UIOptionsWindow uiOptionsWindow;
        [SerializeField] private UIServerConnectLoader serverConnectLoader;
        
        
        [SerializeField] private Button exitButton;
        [SerializeField] private Button browserButton;
        [SerializeField] private Button optionsButton;
        private NetService netService;

        [Inject]
        private void Construct(ServersListService serversListService, PlayerConfigObject playerConfigObject, NetService netService)
        {
            this.netService = netService;
            uiServersBrowserWindow.Init(serversListService, this);
            uiOptionsWindow.Init(playerConfigObject);
            serverConnectLoader.Init(netService);
            
            
            windowsManager.AddWindows(uiServersBrowserWindow, uiOptionsWindow);
            
            exitButton.onClick.AddListener(Exit);
            browserButton.onClick.AddListener(delegate
            {
                windowsManager.OpenWindow(uiServersBrowserWindow);
            });
            
            optionsButton.onClick.AddListener(delegate
            {
                windowsManager.OpenWindow(uiOptionsWindow);
            });
        }


        public void Exit()
        {
            if (Application.isEditor)
            {
                
            }
            else
            {
                Application.Quit();
            }
        }

        public void ConnectToServer(ServersListService.ServerData serverData)
        {
            netService.ConnectToServer();
        }
    }
}
