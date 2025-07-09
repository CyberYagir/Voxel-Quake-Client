using System;
using System.Collections.Generic;
using LightServer.Base.PlayersModule;
using LiteNetLib;
using ServerLibrary.Structs;
using UnityEngine;
using Vector3Int = UnityEngine.Vector3Int;

namespace Content.Scripts.Services.Net
{
    public class NetServiceBlocks : NetServiceModule
    {
        private Dictionary<NetVector3Int, NetChunkData> startChangedChunks = new Dictionary<NetVector3Int, NetChunkData>();
        
        
        public NetServiceBlocks(NetService netService) : base(netService)
        {
            netService.OnCommandReceived += NetServiceOnOnCommandReceived;
        }

        public Dictionary<NetVector3Int, NetChunkData> StartChangedChunks => startChangedChunks;

        public override void Dispose()
        {
            netService.OnCommandReceived -= NetServiceOnOnCommandReceived;
        }

        private void NetServiceOnOnCommandReceived(ECMDName cmdType, NetPeer peer, NetPacketReader reader)
        {
            switch (cmdType)
            {
                case ECMDName.GetChangedBlocks:
                    HandleGetChangedBlocks(reader);
                    break;
            }
        }

        private void HandleGetChangedBlocks(NetPacketReader reader)
        {
            PacketsManager.ReadNetChunksData(reader, startChangedChunks);

            Debug.Log(startChangedChunks.Count);
        }

        public void RPCRemoveBlock(Dictionary<NetVector3Int, NetChunkData> netChunksData)
        {
            netService.Peer.RPCRemoveBlocks(netChunksData);
        }
    }
}
