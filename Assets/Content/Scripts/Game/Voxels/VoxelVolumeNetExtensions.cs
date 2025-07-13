using System.Collections.Generic;
using Content.Scripts.Services.Net;
using LightServer.Base.PlayersModule;
using UnityEngine;

namespace Content.Scripts.Game.Voxels
{
    public static class VoxelVolumeNetExtensions
    {
        private static List<Dictionary<int, NetBlockData>> netChunksPool = new();
        private static Dictionary<NetVector3Int, NetChunkData> netChunksData = new();

        public static void AddDestroyedBlockNetPool(this VoxelVolume volume, Vector3Int chunkChunkPos, int index, Block block)
        {
            NetVector3Int pos = new (chunkChunkPos.x, chunkChunkPos.y, chunkChunkPos.z);
            if (!netChunksData.ContainsKey(pos))
            {
                var dic = GetNetChunkDictionary(netChunksData.Count);
                dic.Clear();
                netChunksData.Add(pos, new NetChunkData(dic));
            }

            if (!netChunksData[pos].blocks.ContainsKey(index))
            {
                netChunksData[pos].blocks.Add(index, new NetBlockData(block.type, block.materialId));
            }
            else
            {
                netChunksData[pos].blocks[index] = new NetBlockData(block.type, block.materialId);
            }
        }

        private static Dictionary<int, NetBlockData> GetNetChunkDictionary(int count)
        {
            if (count >= netChunksPool.Count)
            {
                netChunksPool.Add(new Dictionary<int, NetBlockData>(256));
            }

            return netChunksPool[count];
        }
        
        public static void ModifiedNetChunksDispose(this VoxelVolume volume, NetServiceBlocks netBlocks, bool isMine)
        {
            if (isMine)
            {
                netBlocks.RPCRemoveBlock(VoxelVolumeNetExtensions.netChunksData);
            }

            netChunksData.Clear();
        }

    }
}