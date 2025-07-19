using System;
using System.Collections.Generic;
using Content.Scripts.Scriptable;
using LightServer.Base.PlayersModule;
using LiteNetLib;
using ServerLibrary.Structs;
using UnityEngine;

namespace Content.Scripts.Services.Net
{
    [System.Serializable]
    public class NetServicePlayers : NetServiceModule
    {
        [System.Serializable]
        public class ListPlayerData
        {
            [SerializeField] private int playerId;
            [SerializeField] private string nickName;
            [SerializeField] private bool isInited;
            [SerializeField] private Color color;

            public ListPlayerData(int playerId, string nickName, bool isInited, Color color)
            {
                this.playerId = playerId;
                this.nickName = nickName;
                this.isInited = isInited;
                this.color = color;
            }

            public bool IsInited => isInited;

            public string NickName => nickName;

            public int PlayerId => playerId;

            public Color Color => color;
        }

        [SerializeField] private List<ListPlayerData> players = new List<ListPlayerData>();


        private PlayerConfigObject playerConfigObject;
        private NetPeer localPlayer;
        public event Action<Vector3, Vector3, int> OnRespawnPlayer;
        public event Action<Vector3, Vector3, Vector3, float, int> OnUpdatePlayerTransform;
        public event Action<int> OnDestroyPlayer;
        public event Action<int, EWeaponType> OnPlayerChangeWeapon;


        public NetServicePlayers(NetService netService, PlayerConfigObject playerConfigObject) : base(netService)
        {
            this.playerConfigObject = playerConfigObject;
            netService.OnCommandReceived += NetServiceOnOnCommandReceived;
            netService.Listener.PeerConnectedEvent += ListenerOnPeerConnectedEvent;
        }

        public override void Dispose()
        {
            base.Dispose();

            netService.OnCommandReceived -= NetServiceOnOnCommandReceived;
            netService.Listener.PeerConnectedEvent -= ListenerOnPeerConnectedEvent;
        }

        private void NetServiceOnOnCommandReceived(ECMDName cmdType, NetPeer peer, NetPacketReader reader)
        {
            switch (cmdType)
            {
                case ECMDName.GetPlayersList:
                    HandleCMDGetPlayersList(reader);
                    break;
                case ECMDName.RespawnPlayer:
                    HandleCMDRespawnPlayer(reader);
                    break;
                case ECMDName.UpdatePlayerTransform:
                    HandleCMDUpdatePlayerTransform(reader);
                    break;
                
                case ECMDName.DestroyPlayer:
                    HandleCMDDestroyPlayer(reader);
                    break;
                
                case ECMDName.ChangeWeapon:
                    HandleCMDChangeWeapon(reader);
                    break;
            }
        }

        private void HandleCMDChangeWeapon(NetPacketReader reader)
        {
            var weapon = (EWeaponType)reader.GetByte();
            var ownerID = reader.GetInt();

            OnPlayerChangeWeapon?.Invoke(ownerID, weapon);
        }

        private void HandleCMDDestroyPlayer(NetPacketReader reader)
        {
            var ownerId = reader.GetInt();
            
            OnDestroyPlayer?.Invoke(ownerId);
        }

        private void HandleCMDUpdatePlayerTransform(NetPacketReader reader)
        {
            var ownerId = reader.GetInt();
            var pos = PacketsManager.ReadVector(reader);
            var rot = PacketsManager.ReadVector(reader);
            var vel = PacketsManager.ReadVector(reader);
            var cameraX = reader.GetFloat();
            
            OnUpdatePlayerTransform?.Invoke(
                new Vector3(pos.x, pos.y, pos.z),
                new Vector3(rot.x, rot.y, rot.z),
                new Vector3(vel.x, vel.y, vel.z),
                cameraX,
                ownerId
            );
        }

        private void HandleCMDRespawnPlayer(NetPacketReader reader)
        {
            var ownerId = reader.GetInt();
            var pos = PacketsManager.ReadVector(reader);
            var rot = PacketsManager.ReadVector(reader);

            OnRespawnPlayer?.Invoke(
                new Vector3(pos.x, pos.y, pos.z),
                new Vector3(rot.x, rot.y, rot.z),
                ownerId
            );
        }

        private void ListenerOnPeerConnectedEvent(NetPeer peer)
        {
            ColorUtility.TryParseHtmlString(playerConfigObject.PlayerColor, out Color color);
            peer.RPCInitPlayer(playerConfigObject.PlayerName, new NetVector3(color.r, color.g, color.b));
        }


        private void HandleCMDGetPlayersList(NetPacketReader reader)
        {
            var playersCount = reader.GetInt();

            players.Clear();

            for (int i = 0; i < playersCount; i++)
            {
                var id = reader.GetInt();
                var nickName = reader.GetString();
                var isInited = reader.GetBool();
                
                var netColor = PacketsManager.ReadVector(reader);
                var color = new Color(netColor.x, netColor.y, netColor.z, 1);
                
                players.Add(new ListPlayerData(id, nickName, isInited, color));
            }
        }

        public void RespawnPlayerRPC(Vector3 spawnPosition, Vector3 spawnEulerAngles)
        {
            netService.Peer.RPCSpawnPlayer(
                new(spawnPosition.x, spawnPosition.y, spawnPosition.z),
                new(spawnEulerAngles.x, spawnEulerAngles.y, spawnEulerAngles.z)
            );
        }

        public void RPCUpdatePlayerTransform(Vector3 pos, Vector3 rot, Vector3 vel, float cameraX)
        {
            netService.Peer.RPCUpdatePlayerTransform(
                new(pos.x, pos.y, pos.z), 
                new(rot.x, rot.y, rot.z),
                new(vel.x, vel.y, vel.z), 
                cameraX
                );
        }

        public bool HasPlayer(int senderID)
        {
            return players.Find(x => x.PlayerId == senderID) != null;
        }

        public string GetPlayerNickName(int senderID)
        {
            return players.Find(x => x.PlayerId == senderID).NickName;
        }

        public void RPCChangeWeapon(EWeaponType objType)
        {
            netService.Peer.RPCChangeWeapon(objType);
        }

        public Color GetPlayerColor(int netObjectPeerID)
        {
            var player = players.Find(x => x.PlayerId == netObjectPeerID);
            if (player != null)
            {
                return player.Color;
            }

            return Color.gray;
        }
    }
}
