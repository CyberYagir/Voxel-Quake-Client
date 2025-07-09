using System.Collections.Generic;
using Content.Scripts.Menu.Services;
using TMPro;
using UnityEngine;

namespace Content.Scripts.Menu.UI
{
    public class UIServersBrowserWindow : WindowBase
    {
        [SerializeField] private UIServersBrowserItem item;
        [SerializeField] private TMP_Text noServers;
        [SerializeField] private TMP_Text fetching;

        private List<UIServersBrowserItem> items = new List<UIServersBrowserItem>();
        private ServersListService serversListService;
        private MenuUIService menuUIService;

        public void Init(ServersListService serversListService, MenuUIService menuUIService)
        {
            this.menuUIService = menuUIService;
            this.serversListService = serversListService;
            item.gameObject.SetActive(false);
            serversListService.OnServerListUpdated += ServersListServiceOnOnServerListUpdated;
            serversListService.OnServerStarted += ServersListServiceOnOnServerStarted;
        }

        public override void Show()
        {
            base.Show();
            
            serversListService.ResetTime();
        }

        private void ServersListServiceOnOnServerStarted()
        {
            if (items.Count == 0)
            {
                fetching.gameObject.SetActive(true);
                noServers.gameObject.SetActive(false);
            }
        }

        private void ServersListServiceOnOnServerListUpdated()
        {
            fetching.gameObject.SetActive(false);
            for (int i = 0; i < serversListService.ServerDatas.Count; i++)
            {
                var data = serversListService.ServerDatas[i];


                if (items.Count <= i)
                {
                    Instantiate(item, item.transform.parent)
                        .With(x => items.Add(x))
                        .With(x => x.gameObject.SetActive(true));
                }
                
                items[i].Init(data, this);
            }

            for (int i = serversListService.ServerDatas.Count; i < items.Count; i++)
            {
                items[i].gameObject.SetActive(false);
            }
            
            noServers.gameObject.SetActive(serversListService.ServerDatas.Count == 0);
            if (serversListService.ServerDatas.Count == 0)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    noServers.text = "No Connection";
                }
                else
                {
                    noServers.text = "No Servers";
                }
            }
        }

        public void ConnectToServer(ServersListService.ServerData serverData)
        {
            menuUIService.ConnectToServer(serverData);
        }
    }
}
