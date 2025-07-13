using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Vector3Int = UnityEngine.Vector3Int;

namespace Content.Scripts.Game.Voxels
{
    public static class VoxelVolumeExtensions
    {
        private static HashSet<IMeshDrawable> modifiedChunks = new HashSet<IMeshDrawable>(6);

        public static void AddBlocksFromBounds(this VoxelVolume volume, Transform objTransform, Bounds bounds, byte blockType = 1, byte materialId = 0)
        {
            // Размер одного вокселя
            float voxelSize = volume.VoxelSize/2f;

            // Находим размер воксельного куба в локальных координатах объекта
            Vector3 size = objTransform.lossyScale/voxelSize;

            Debug.Log(size);

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        var localPos = new Vector3(x / size.x, y / size.y, z / size.z) - Vector3.one / 2f;
                        var worldVoxelPosition = objTransform.TransformPoint(localPos);
                        //
                        // var g = new GameObject();
                        // g.transform.position = worldVoxelPosition;
                        // g.transform.parent = objTransform;
                    
                        SetBlock(volume, worldVoxelPosition, blockType, materialId, out var chunk, false, null);

                        if (chunk != null)
                        {
                            ModifyChunk(volume, chunk);
                        }
                    }
                }
            }
        }

        private static Dictionary<int, int> destoryedBlocks = new Dictionary<int, int>();
        public static Dictionary<int, int> DestroyBlocksInRadius(this VoxelVolume volume, Vector3 hitInfoPoint, float destructionRadius, byte damage)
        {
            destoryedBlocks.Clear();
            var min = hitInfoPoint - Vector3.one * destructionRadius;
            var max = hitInfoPoint + Vector3.one * destructionRadius;

            for (float x = min.x; x < max.x; x += volume.VoxelSize)
            {
                for (float y = min.y; y < max.y; y += volume.VoxelSize)
                {
                    for (float z = min.z; z < max.z; z += volume.VoxelSize)
                    {
                        var pos = new Vector3(x, y, z);
                        var dist = Vector3.Distance(pos, hitInfoPoint);
                        if (dist < destructionRadius)
                        {
                            DamageBlock(volume, pos, (1f -(dist/destructionRadius)) * damage, out var material, out var isDestroyed);
                            if (isDestroyed)
                            {
                                if (!destoryedBlocks.ContainsKey(material))
                                {
                                    destoryedBlocks.Add(material, 0);
                                }
                                destoryedBlocks[material]++;
                            }
                        }
                    }
                }
            }
        

            return destoryedBlocks;
        }

        private static void DamageBlock(this VoxelVolume volume, Vector3 pos, float damage, out int material, out bool isDestroyed)
        {
            var index = GetBlock(volume, pos, out var chunk);
            isDestroyed = false;
            material = -1;
            if (index != -1)
            {
                if (chunk.BlocksData[index].type != 0)
                {
                    chunk.BlocksData[index].health -= damage;
                    if (chunk.BlocksData[index].health <= 0)
                    {
                        chunk.BlocksData[index].type = 0;
                        isDestroyed = true;
                        material = chunk.BlocksData[index].materialId;

                        volume.AddDestroyedBlockNetPool(chunk.ChunkPos, index, chunk.BlocksData[index]);
                        ModifyChunk(volume, chunk);
                    }
                    return;
                }
            }
            for (var i = 0; i < volume.DynamicChunks.Count; i++)
            {
                if (volume.DynamicChunks[i] != null)
                {
                    if (volume.DynamicChunks[i].DamageBlock(pos, damage, out material, out isDestroyed))
                    {
                        ModifyChunk(volume, volume.DynamicChunks[i]);
                        break;
                    }
                }
            }
        }
    
        public static void ModifiedChunksDispose(this VoxelVolume volume)
        {
            foreach (var chunk in modifiedChunks)
            {
                chunk.ModifyChunk();
                if (chunk is DynamicChunkVolume k)
                {
                    k.CalculateChunkParts();
                }
            }
        
        
        
            modifiedChunks.Clear();
        }

    
        public static void SetBlock(this VoxelVolume volume, Vector3 worldPosition, byte blockType, byte materialId,
            out ChunkVolume chunk, bool updateChunk, List<int> ignoredMaterials)
        {
            // Прямое вычисление локальной позиции без Transform (если VoxelVolume в origin и без поворота)
            Vector3 volumeOrigin = volume.transform.position;
            Vector3 localPos = worldPosition - volumeOrigin;
    
            // Альтернативно, если нужны трансформации, кэшируйте inverse matrix:
            // Vector3 localPos = volume.cachedInverseMatrix.MultiplyPoint3x4(worldPosition);
    
            // Прямое вычисление координат без промежуточных Vector3Int
            float invVoxelSize = 1f / volume.VoxelSize;
            int voxelX = Mathf.RoundToInt(localPos.x * invVoxelSize);
            int voxelY = Mathf.RoundToInt(localPos.y * invVoxelSize);
            int voxelZ = Mathf.RoundToInt(localPos.z * invVoxelSize);
    
            // Inline вычисление координат чанка и локальной позиции
            Vector3Int chunkSize = volume.ChunkSize;
            int chunkX = voxelX >= 0 ? voxelX / chunkSize.x : (voxelX - chunkSize.x + 1) / chunkSize.x;
            int chunkY = voxelY >= 0 ? voxelY / chunkSize.y : (voxelY - chunkSize.y + 1) / chunkSize.y;
            int chunkZ = voxelZ >= 0 ? voxelZ / chunkSize.z : (voxelZ - chunkSize.z + 1) / chunkSize.z;
    
            // Проверка границ с ранним выходом
            if (chunkX < 0 || chunkX >= volume.BoundsSize.x ||
                chunkY < 0 || chunkY >= volume.BoundsSize.y ||
                chunkZ < 0 || chunkZ >= volume.BoundsSize.z)
            {
                chunk = null;
                return;
            }
    
            // Локальные координаты в чанке
            int localX = voxelX - chunkX * chunkSize.x;
            int localY = voxelY - chunkY * chunkSize.y;
            int localZ = voxelZ - chunkZ * chunkSize.z;
    
            Vector3Int chunkCoord = new Vector3Int(chunkX, chunkY, chunkZ);
            Vector3Int localVoxelInChunk = new Vector3Int(localX, localY, localZ);
    
            chunk = volume.GetChunkByPos(chunkCoord);
            var index = chunk.GetBlockIndex(localVoxelInChunk);
            if (index != -1 && ignoredMaterials != null)
            {
                if (ignoredMaterials.Contains(chunk.BlocksData[index].materialId))
                {
                    return;
                }
            }

            if (chunk != null && chunk.SetBlock(localVoxelInChunk, blockType, materialId))
            {
                if (updateChunk)
                {
                    chunk.ModifyChunk();
                }
                else
                {
                    ModifyChunk(volume, chunk);
                }
            }
        }

        public static void ModifyChunk(this VoxelVolume volume, IMeshDrawable chunk)
        {
            modifiedChunks.Add(chunk);
        }

        public static int GetBlock(this VoxelVolume volume, Vector3 worldPosition, out ChunkVolume chunk)
        {
            // Преобразуем мировую позицию в локальную координату относительно VoxelVolume
            Vector3 localPos = volume.transform.InverseTransformPoint(worldPosition);
            // Преобразуем в координаты вокселей (учитывая размер вокселя)
            Vector3Int voxelPos = Vector3Int.RoundToInt(localPos / volume.VoxelSize);

            return GetBlockByVoxelPos(volume, out chunk, voxelPos);
        }

        public static int GetBlockByVoxelPos(this VoxelVolume volume, out ChunkVolume chunk, Vector3Int voxelPos)
        {
            Vector3Int chunkCoord = GetChunkCoordinate(voxelPos, volume.ChunkSize);
            Vector3Int localVoxelInChunk = GetLocalVoxelInChunk(voxelPos, volume.ChunkSize);

            // Проверяем, что чанк существует в пределах boundsSize
            if (chunkCoord.x >= 0 && chunkCoord.x < volume.BoundsSize.x &&
                chunkCoord.y >= 0 && chunkCoord.y < volume.BoundsSize.y &&
                chunkCoord.z >= 0 && chunkCoord.z < volume.BoundsSize.z)
            {
                chunk = volume.GetChunkByPos(chunkCoord);
                if (chunk != null)
                {
                    var index = chunk.GetBlockIndex(localVoxelInChunk);

                    return index;
                }
            }
        
            chunk = null;
            return -1;
        }


        /// <summary>
        /// Получает координату чанка по позиции вокселя
        /// </summary>
        private static Vector3Int GetChunkCoordinate(Vector3Int voxelPosition, Vector3Int chunkSize)
        {
            return new Vector3Int(
                Mathf.FloorToInt((float)voxelPosition.x / chunkSize.x),
                Mathf.FloorToInt((float)voxelPosition.y / chunkSize.y),
                Mathf.FloorToInt((float)voxelPosition.z / chunkSize.z)
            );
        }
    
        /// <summary>
        /// Получает локальную позицию вокселя внутри чанка
        /// </summary>
        private static Vector3Int GetLocalVoxelInChunk(Vector3Int voxelPosition, Vector3Int chunkSize)
        {
            return new Vector3Int(
                ((voxelPosition.x % chunkSize.x) + chunkSize.x) % chunkSize.x,
                ((voxelPosition.y % chunkSize.y) + chunkSize.y) % chunkSize.y,
                ((voxelPosition.z % chunkSize.z) + chunkSize.z) % chunkSize.z
            );
        }
    
    
    }
}
