using System;
using System.Collections.Generic;
using Content.Scripts.Game.IO.Structures;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game.Voxels
{
    public partial class DynamicChunkVolume : MonoBehaviour, IMeshDrawable
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshCollider meshCollider;
        [SerializeField] private Vector3Int chunkSize;
        [SerializeField] private float voxelSize;



        public event Action OnDynamicChunkChanged;
        private Block[] blocks;
        private Material[] materials;
        private VoxelVolume voxelVolume;


        public Block[] BlocksData => blocks;
        public Vector3Int ChunkSize => chunkSize;
        public float VoxelSize => voxelSize;
        public Dictionary<Vector3Int, IMeshDrawable> Neighbors => new Dictionary<Vector3Int, IMeshDrawable>();

        public event Action<IMeshDrawable> OnChanged;


        [Inject]
        private void Construct(VoxelVolume voxelVolume)
        {
            this.voxelVolume = voxelVolume;
        }



        public void LoadBlocks(SavedVolume itemVoxData)
        {
            SetDynamicData(itemVoxData.chunkSize.Convert(), itemVoxData.voxelSize);

            if (itemVoxData.chunks.Length > 0)
            {
                voxelVolume.SetBlocksToChunk(itemVoxData.chunks[0], this);
                ModifyChunk();
            }
        }

        public void SetDynamicData(Vector3Int chunkSize, float voxelSize)
        {
            this.chunkSize = chunkSize;
            this.voxelSize = voxelSize;


            blocks = new Block[ChunkSize.x * ChunkSize.y * ChunkSize.z];
            gameObject.SetActive(true);
        }

        [Button]
        public void InitDebug(VoxelVolume voxelVolume)
        {
            this.voxelVolume = voxelVolume;
            voxelSize = voxelVolume.VoxelSize;

            blocks = new Block[ChunkSize.x * ChunkSize.y * ChunkSize.z];


            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].type = 1;
                blocks[i].materialId = 7;
                blocks[i].health = 20;
            }

            ModifyChunk();
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
            return GetBlock(x, y, z);
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
            int index = localVoxelInChunk.x + ChunkSize.x * (localVoxelInChunk.y + ChunkSize.y * localVoxelInChunk.z);

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

        public void SetMaterials(Material[] materials)
        {
            this.materials = materials;
        }

        public bool DamageBlock(Vector3 pos, float damage, out int material, out bool isDestroyed)
        {
            var voxelCoord = Vector3Int.RoundToInt(transform.InverseTransformPoint(pos) / voxelSize);
            material = -1;
            isDestroyed = false;


            if (voxelCoord.x < 0 || 
                voxelCoord.y < 0 || 
                voxelCoord.z < 0 || 
                voxelCoord.x >= chunkSize.x ||
                voxelCoord.y >= chunkSize.y || 
                voxelCoord.z >= chunkSize.z) return false;
            

            var index = GetBlockIndex(voxelCoord);
            if (index != -1)
            {
                if (BlocksData[index].type != 0)
                {
                    material = BlocksData[index].materialId;
                    BlocksData[index].health -= damage;
                    
                    Debug.DrawRay(pos, Vector3.up * voxelSize, Color.Lerp(Color.red, Color.green, BlocksData[index].health/50f), 5f);
                    if (BlocksData[index].health <= 0)
                    {
                        BlocksData[index].type = 0;
                        isDestroyed = true;
                        OnDynamicChunkChanged?.Invoke();
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
