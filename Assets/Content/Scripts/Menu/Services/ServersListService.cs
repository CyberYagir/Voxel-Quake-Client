using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace Content.Scripts.Menu.Services
{
    public class ServersListService : MonoBehaviour
    {
        [System.Serializable]
        public struct Response
        {
            public bool error;
            public string message;
            public Data data;
            
            public struct Data
            {
                public List<ServerData> servers;
            }
            
        }
        
        [System.Serializable]
        public struct ServerData
        {
            public int id;
            public string ip;
            public int port;
            public int players_count;
            public int max_players_count;
            public int last_time;
            public string title;
            public string map;
        }

        [SerializeField] private List<ServerData> serverDatas;
        [SerializeField] private Response response;
        private float delay;

        public event Action OnServerListUpdated;
        public event Action OnServerStarted;

        public List<ServerData> ServerDatas => serverDatas;

        [Inject]
        private void Construct()
        {
            StartCoroutine(GetServersLoop());
        }

        public void ResetTime()
        {
            delay = 0;
        }

        IEnumerator GetServersLoop()
        {
            while (true)
            {
                delay = 30;
                UnityWebRequest request = UnityWebRequest.Get("https://yagir.xyz/serverslist/?mode=2");
                print("Getting servers list");
                OnServerStarted?.Invoke();
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    response = JsonConvert.DeserializeObject<Response>(request.downloadHandler.text);

                    if (response.error)
                    {
                        delay = 2;
                    }
                    else
                    {
                        serverDatas = response.data.servers;
                        OnServerListUpdated?.Invoke();
                    }
                }
                else
                {
                    delay = 2f;
                }

                float time = 0;

                while (time < delay)
                {
                    time += Time.deltaTime;
                    yield return null;
                }
                
                
            }
        }
    }
}
