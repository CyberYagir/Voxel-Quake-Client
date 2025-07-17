using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Content.Scripts.Game.Services;
using Content.Scripts.Scriptable;
using DG.Tweening;
using LiteNetLib;
using ServerLibrary.Structs;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Services.Net
{
    public class NetService : SerializedMonoBehaviour
    {
        [SerializeField] private List<NetServiceModule> modules = new List<NetServiceModule>();
        
        private NetManager client;
        private EventBasedNetListener listener; // ⬅️ обязательно сохранить
        private NetPeer peer;
        private string reason;
        private PlayerConfigObject playerConfig;
        private ScenesService scenesService;
        private PrefabSpawnerFabric fabric;
        private ProjectilesConfigObject projectilesConfig;

        public event Action OnPlayerConnected;
        public event Action OnPlayerConnectionStart;
        public event Action OnPlayerDisconnected;
        public event Action<ECMDName, NetPeer, NetPacketReader> OnCommandReceived;

        public EventBasedNetListener Listener => listener;

        public NetPeer Peer => peer;

        public PrefabSpawnerFabric SpawnedFabric => fabric;

        [Inject]
        private void Construct(ScenesService scenesService, PlayerConfigObject playerConfigObject, ProjectilesConfigObject projectilesConfig)
        {
            this.projectilesConfig = projectilesConfig;
            this.scenesService = scenesService;
            this.playerConfig = playerConfigObject;
            OnPlayerConnected += delegate
            {
                scenesService.LoadScene(ScenesService.EScene.Game);
            };
            OnPlayerDisconnected += delegate
            {
                scenesService.LoadScene(ScenesService.EScene.Menu);
            };
        }
        
        public void ConnectToServer()
        {
            
            OnPlayerConnectionStart?.Invoke();
            
            
            listener = new EventBasedNetListener(); // ⬅️ сохраняем
            client = new NetManager(listener);
            client.Start();

            Debug.Log("Connecting to server...");
            client.Connect("127.0.0.1", 9050, "SomeConnectionKey");
            
            //////////////////////
            
            listener.PeerConnectedEvent += OnConnectedEvent;
            listener.NetworkErrorEvent += OnNetworkErrorEvent;
            listener.PeerDisconnectedEvent += OnPeerDisconnectedEvent;
            listener.NetworkReceiveEvent += ListenerOnNetworkReceiveEvent;

            ConfigureModules();
        }

        private void ListenerOnNetworkReceiveEvent(NetPeer netPeer, NetPacketReader reader, byte channel, DeliveryMethod deliverymethod)
        {
            var cmdType = (ECMDName)reader.GetByte();
            var id = reader.GetInt();
            
            OnCommandReceived?.Invoke(cmdType, netPeer, reader);
            
            reader.Recycle();
        }



        private void ConfigureModules()
        {
            modules.Clear();

            for (int i = 0; i < modules.Count; i++)
            {
                modules[i].Dispose();
            }

            modules.Add(new NetServiceClientState(this, scenesService));
            modules.Add(new NetServiceServer(this));
            modules.Add(new NetServicePlayers(this, playerConfig));
            modules.Add(new NetServiceProjectiles(this, projectilesConfig, fabric));
            modules.Add(new NetServiceBlocks(this));
            modules.Add(new NetServiceMapItems(this));
            modules.Add(new NetServiceChat(this));
        }



        private void OnPeerDisconnectedEvent(NetPeer netPeer, DisconnectInfo disconnectinfo)
        {
            OnPlayerDisconnected?.Invoke();
        }

        private void OnNetworkErrorEvent(IPEndPoint endpoint, SocketError socketerror)
        {
            OnPlayerDisconnected?.Invoke();
        }

        private void OnConnectedEvent(NetPeer peer)
        {
            this.peer = peer;
            OnPlayerConnected?.Invoke();
        }


        private void Update()
        {
            if (client != null && client.IsRunning)
            {
                client.PollEvents();
            }
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        public void Disconnect(string reason = null)
        {
            this.reason = reason;
            if (client != null && client.IsRunning)
            {
                client.Stop();
            }
        }

        public T GetModule<T>() where T : NetServiceModule
        {
            return modules.Find(x => x.GetType().Name == typeof(T).Name) as T;
        }

        public T InstantiateNetObject<T>(GameObject obj, Vector3 pos, Vector3 rot, int owner) where T : MonoBehaviour
        {
            var spawned = Instantiate(obj, pos, Quaternion.Euler(rot));
            spawned.GetComponent<NetObject>().Init(owner, peer.RemoteId);
            fabric.InjectComponent(spawned.gameObject);
            return spawned.GetComponent<T>();
        }

        public void SetFabric(PrefabSpawnerFabric fabric)
        {
            this.fabric = fabric;
        }
    }
}
