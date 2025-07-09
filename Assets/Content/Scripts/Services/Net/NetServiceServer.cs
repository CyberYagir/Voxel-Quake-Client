using System;
using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using ServerLibrary.Structs;
using UnityEngine;

namespace Content.Scripts.Services.Net
{
    [System.Serializable]
    public class NetServiceServer : NetServiceModule
    {
        [System.Flags]
        public enum EServerParts
        {
            Map = 8,
            Timer = 16,
            GameState = 32,
            Blocks = 64,
            
            Loaded = 256
        }
        
        [System.Serializable]
        public class ServerData
        {
            [SerializeField] private string mapName;
            [SerializeField] private float timer;
            [SerializeField] private EGameState gameState;

            public EGameState GameState => gameState;

            public float Timer => timer;

            public string MapName => mapName;

            public void SetMap(string map)
            {
                mapName = map;
            }

            public void SetTime(int time)
            {
                timer = time;
            }
            
            public void SetGameState(EGameState state)
            {
                gameState = state;
            }
        }
        
        [SerializeField] private ServerData serverData = new ServerData();
        [SerializeField] private EServerParts loadedStates = 0;
        [SerializeField] private bool isLoaded;
        
        
        public string MapName => serverData.MapName;

        public bool IsLoaded => isLoaded;


        public event Action OnServerLoaded;
        
        private List<Action> afterLoading = new List<Action>(); 
        
        public NetServiceServer(NetService netService) : base(netService)
        {
            netService.OnCommandReceived += NetServiceOnOnCommandReceived;
        }
        
        public override void Dispose()
        {
            base.Dispose();
            
            netService.OnCommandReceived -= NetServiceOnOnCommandReceived;
        }

        private void NetServiceOnOnCommandReceived(ECMDName cmdType, NetPeer peer, NetPacketReader reader)
        {
            switch (cmdType)
            {
                case ECMDName.GetMap:
                    HandleCMDGetGetMap(reader);
                    break;
                case ECMDName.GetTime:
                    HandleCMDGetTime(reader);
                    break;
                case ECMDName.GetGameState:
                    HandleCMDGetGameState(reader);
                    break;
                case ECMDName.GetChangedBlocks:
                    HandleCMDGetChangedBlocks(reader);
                    break;
            }
        }

        private void HandleCMDGetChangedBlocks(NetPacketReader reader)
        {
            AddLoadedState(EServerParts.Blocks);
        }

        private void AddLoadedState(EServerParts state)
        {
            if (!loadedStates.HasFlag(state) && !loadedStates.HasFlag(EServerParts.Loaded))
            {
                loadedStates |= state;

                if (
                    loadedStates.HasFlag(EServerParts.Timer) &&
                    loadedStates.HasFlag(EServerParts.Map) &&
                    loadedStates.HasFlag(EServerParts.GameState) &&
                    loadedStates.HasFlag(EServerParts.Blocks))
                {
                    loadedStates |= EServerParts.Loaded;

                    OnServerLoaded?.Invoke();
                    for (int i = 0; i < afterLoading.Count; i++)
                    {
                        afterLoading[i]?.Invoke();
                    }
                    isLoaded = true;
                }
            }
        }
        
        private void HandleCMDGetGameState(NetPacketReader reader)
        {
            serverData.SetGameState((EGameState)reader.GetByte());
            AddLoadedState(EServerParts.GameState);
        }

        private void HandleCMDGetTime(NetPacketReader reader)
        {
            var time = reader.GetInt();
            serverData.SetTime(time);
            AddLoadedState(EServerParts.Timer);
        }

        private void HandleCMDGetGetMap(NetPacketReader reader)
        {
            serverData.SetMap(reader.GetString());
            AddLoadedState(EServerParts.Map);
        }

        public void LaunchAfterLoad(Action action)
        {
            if (isLoaded)
            {
                action?.Invoke();
            }
            else
            {
                afterLoading.Add(action);
            }
        }
    }
}