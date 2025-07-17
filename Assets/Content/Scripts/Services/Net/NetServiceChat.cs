using System;
using LiteNetLib;
using ServerLibrary.Structs;
using UnityEngine;

namespace Content.Scripts.Services.Net
{
    [System.Serializable]
    public class NetServiceChat : NetServiceModule
    {
        private NetServicePlayers netServicePlayers;
        public event Action<string, string> OnMessageReceived;

        public NetServiceChat(NetService netService) : base(netService)
        {
            netServicePlayers = netService.GetModule<NetServicePlayers>();
            netService.OnCommandReceived += NetServiceOnOnCommandReceived;
        }

        public override void Dispose()
        {
            netService.OnCommandReceived -= NetServiceOnOnCommandReceived;
        }

        private void NetServiceOnOnCommandReceived(ECMDName cmdType, NetPeer peer, NetPacketReader reader)
        {
            switch (cmdType)
            {
                case ECMDName.SendChatMessage:
                        HandleCMDSendChatMessage(peer, reader);
                    break;
            }
        }

        private void HandleCMDSendChatMessage(NetPeer peer, NetPacketReader reader)
        {
            var senderID = reader.GetInt();
            var message = reader.GetString();

            if (netServicePlayers.HasPlayer(senderID))
            {
                OnMessageReceived?.Invoke(netServicePlayers.GetPlayerNickName(senderID), message);
            }
        }

        public void SendMessageRPC(string message)
        {
            netService.Peer.RPCSendChatMessage(message);
        }
    }
}