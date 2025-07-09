using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Content.Scripts.Game.Voxels
{
    public interface IMeshDrawable
    {
        public Block[] BlocksData { get; }

        public event Action<IMeshDrawable> OnChanged;
    
        public void SetMesh(Mesh mesh);
    
        Vector3Int ChunkSize { get; }
    
        float VoxelSize { get; }
        Dictionary<Vector3Int, IMeshDrawable> Neighbors { get; }

        public Block GetBlock(int x, int y, int z);

        public Block GetBlockWithNeighbors(int x, int y, int z, ref List<IMeshDrawable> modifiedNeighboursChunks);

        public bool IsEmpty();
        bool HasMesh();
        void SetMaterials(Material[] materials);
        void ModifyChunk();
    }

    public class ChunkVolume : MonoBehaviour, IMeshDrawable
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshCollider meshCollider;
        [SerializeField] private Vector3Int chunkPos;
    
        private VoxelVolume voxelVolume;
        private Block[] blocks;
        private Dictionary<Vector3Int, IMeshDrawable> neighbors = new Dictionary<Vector3Int, IMeshDrawable>();
        private Material[] materials;


        public Block[] BlocksData => blocks;
        public Vector3Int ChunkSize => voxelVolume.ChunkSize;
        public float VoxelSize => voxelVolume.VoxelSize;
        public Dictionary<Vector3Int, IMeshDrawable> Neighbors => neighbors;

        public Vector3Int ChunkPos => chunkPos;

        public event Action<IMeshDrawable> OnChanged;
    
    
        public void Init(VoxelVolume voxelVolume, Vector3Int chunkPos)
        {
            this.chunkPos = chunkPos;
            this.voxelVolume = voxelVolume;
        
            gameObject.SetActive(true);

            blocks = new Block[ChunkSize.x * ChunkSize.y * ChunkSize.z];
        }


        [Button]
        public void DestroyDebug()
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].type = 0;
            }

            ModifyChunk();
        }
    
        public void ModifyChunk()
        {
            OnChanged?.Invoke(this);
        }
    
        public Block GetBlock(int x, int y, int z)
        {
            if (x < 0 || x >= ChunkSize.x || y < 0 || y >= ChunkSize.y || z < 0 || z >= ChunkSize.z)
                return default;

            int index = x + ChunkSize.x * (y + ChunkSize.y * z);
            return blocks[index];
        }
    
        public Block GetBlockWithNeighbors(int x, int y, int z, ref List<IMeshDrawable> modifiedNeighboursChunks)
        {
            Vector3Int size = ChunkSize;

            // Если координаты внутри текущего чанка - возвращаем блок
            if (x >= 0 && x < size.x &&
                y >= 0 && y < size.y &&
                z >= 0 && z < size.z)
            {
                return GetBlock(x, y, z);
            }

            // Вышли за границу - ищем соседний чанк
            Vector3Int neighborOffset = Vector3Int.zero;
            Vector3Int localPos = new Vector3Int(x, y, z);

            // Определяем смещение чанка и локальные координаты
            // Только для положительных чисел, которые вышли за границы
            if (x >= size.x)
            {
                neighborOffset.x = 1;
                localPos.x = x - size.x;
            }

            if (y >= size.y)
            {
                neighborOffset.y = 1;
                localPos.y = y - size.y;
            }

            if (z >= size.z)
            {
                neighborOffset.z = 1;
                localPos.z = z - size.z;
            }

            // Если все координаты остались в пределах [0, size), то это ошибка логики
            if (neighborOffset == Vector3Int.zero)
            {
                Debug.LogError($"Unexpected coordinates: ({x}, {y}, {z}) for chunk size {size}");
                return new Block { type = 0 }; // Возвращаем воздух
            }

            // Ищем соседний чанк
            Vector3Int neighborChunkPos = chunkPos + neighborOffset;
    
            if (neighbors.TryGetValue(neighborChunkPos, out var neighborChunk))
            {
                // Добавляем в список модифицированных соседей
                if (!modifiedNeighboursChunks.Contains(neighborChunk))
                {
                    modifiedNeighboursChunks.Add(neighborChunk);
                }

                // Возвращаем блок из соседнего чанка
                return neighborChunk.GetBlock(localPos.x, localPos.y, localPos.z);
            }

            // Соседнего чанка нет - возвращаем пустой блок (воздух)
            return new Block { type = 0 };
        }


        // public void OnDrawGizmos()
        // {
        //     Gizmos.DrawWireCube(
        //         voxelVolume.GetChunkPosition(chunkPos.x, chunkPos.y, chunkPos.z),
        //         (Vector3)voxelVolume.ChunkSize * voxelVolume.VoxelSize
        //     );
        // }
        public void SetMesh(Mesh mesh)
        {
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
            meshRenderer.sharedMaterials = materials;
        }

        public void AddNeighbours()
        {
            AddNeighbour(Vector3Int.forward);
            AddNeighbour(Vector3Int.back);
            AddNeighbour(Vector3Int.up);
            AddNeighbour(Vector3Int.down);
            AddNeighbour(Vector3Int.left);
            AddNeighbour(Vector3Int.right);
        }

        public void AddNeighbour(Vector3Int dir)
        {
            var chunk = voxelVolume.GetChunkByPos(chunkPos + dir);

            if (chunk != null)
            {
                neighbors.Add(chunkPos + dir, chunk);
            }
        }

        public bool SetBlock(Vector3Int localVoxelInChunk, byte blockType, byte materialId)
        {
            var blockIndex = GetBlockIndex(localVoxelInChunk);
            if (blockIndex != -1)
            {
                BlocksData[blockIndex].type = blockType;
                BlocksData[blockIndex].materialId = materialId;
                return true;
            }
        
            return false;
        }

        public int GetBlockIndex(Vector3Int localVoxelInChunk)
        {
            var blockArray = BlocksData;
            int index = localVoxelInChunk.x + ChunkSize.x *
                (localVoxelInChunk.y + ChunkSize.y * localVoxelInChunk.z);

            if (index >= 0 && index < blockArray.Length)
            {
                return index;
            }
            return -1;
        }

        public bool IsEmpty()
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i].type != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasMesh()
        {
            if (meshFilter.sharedMesh != null)
            {
                return meshFilter.sharedMesh.vertexCount != 0;
            }

            return false;
        }

        public void ChangeLayer(LayerMask layer)
        {
            gameObject.layer = layer;
        }

        public void SetMaterials(Material[] materials)
        {
            this.materials = materials;
        }
    }
}