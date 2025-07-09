using System.Collections.Generic;
using Content.Scripts.Game.Voxels;
using UnityEngine;

namespace Content.Scripts.Game.IO.Structures
{
    [System.Serializable]
    public struct ChunkData
    {
        public Vector3IntData chunkPosition;
        public BlockData[] blocksData; // Массив вместо List

        public ChunkData(Vector3Int chunkPosition, Block[] blocks)
        {
            this.chunkPosition = chunkPosition.Convert();
            
            var tempBlocksData = new List<BlockData>();
            ProcessBlocks(blocks, tempBlocksData);
            
            this.blocksData = tempBlocksData.ToArray();
        }

        // Вынесли логику обработки блоков в отдельный метод для читаемости
        private static void ProcessBlocks(Block[] blocks, List<BlockData> blocksData)
        {
            Block lastBlock = default;
            bool hasLast = false;
            int startID = 0;
            int lineLength = 0;

            for (int i = 0; i < blocks.Length; i++)
            {
                Block block = blocks[i];

                if (block.type == 0)
                {
                    if (lineLength > 0)
                    {
                        blocksData.Add(new BlockData(startID, lastBlock, lineLength));
                        lineLength = 0;
                        hasLast = false;
                    }
                    continue;
                }

                if (!hasLast || !block.Equals(lastBlock))
                {
                    if (lineLength > 0)
                        blocksData.Add(new BlockData(startID, lastBlock, lineLength));

                    startID = i;
                    lineLength = 1;
                    lastBlock = block;
                    hasLast = true;
                }
                else
                {
                    lineLength++;
                }
            }

            if (lineLength > 0)
                blocksData.Add(new BlockData(startID, lastBlock, lineLength));
        }
    }
}