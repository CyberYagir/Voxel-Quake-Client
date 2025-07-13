using System;
using System.Collections.Generic;
using Content.Scripts.Game.IO.Structures;
using Content.Scripts.Game.Scriptable;
using Content.Scripts.Game.Services;
using LightServer.Base.PlayersModule;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game.Voxels
{
    public class VoxelVolume : MonoBehaviour
    {
        [SerializeField] private float voxelSize;
        [SerializeField] private Vector3Int chunkSize;
        [SerializeField] private Vector3Int boundsSize;
        [SerializeField] private DynamicChunkVolume dynamicChunkVolumePrefab;
        [SerializeField] private List<DynamicChunkVolume> dynamicChunks = new List<DynamicChunkVolume>();
        [SerializeField] private ChunkVolume chunkVolume;


        [SerializeField] private Dictionary<Vector3Int, ChunkVolume> chunks = new Dictionary<Vector3Int, ChunkVolume>();
        [SerializeField] private List<VolumeModification> volumeModifications;
        private MaterialListObject materialListObject;
        private PrefabSpawnerFabric spawnerFabric;

        public float VoxelSize => voxelSize;

        public Vector3Int ChunkSize => chunkSize;

        public Vector3Int BoundsSize => boundsSize;

        public Dictionary<Vector3Int, ChunkVolume> Chunks => chunks;

        public List<DynamicChunkVolume> DynamicChunks => dynamicChunks;

        public Vector3 FromLocal(Vector3 pos) => transform.TransformPoint(pos) + (Vector3)chunkSize / 2f;

        public event Action<IMeshDrawable> OnChunkRemoved;
        public event Action<IMeshDrawable> OnChunkAdded;

        [Inject]
        private void Construct(MaterialListObject materialListObject, PrefabSpawnerFabric spawnerFabric)
        {
            this.spawnerFabric = spawnerFabric;
            this.materialListObject = materialListObject;
        }


        [Button]
        private void Init()
        {
            for (int x = 0; x < boundsSize.x; x++)
            {
                for (int y = 0; y < boundsSize.y; y++)
                {
                    for (int z = 0; z < boundsSize.z; z++)
                    {
                        var chunk = Instantiate(chunkVolume, transform);
                        chunk.transform.position =
                            FromLocal(new Vector3(x * chunkSize.x, y * chunkSize.y, z * chunkSize.z) -
                                      (Vector3)chunkSize / 2f) * voxelSize;
                        var pos = new Vector3Int(x, y, z);
                        chunk.Init(this, pos);

                        chunks.Add(pos, chunk);
                    }
                }
            }

            foreach (var chunk in chunks)
            {
                chunk.Value.AddNeighbours();
            }

            for (var i = 0; i < volumeModifications.Count; i++)
            {
                volumeModifications[i].Init(this);
            }
        }
        

        private void OnDrawGizmos()
        {

            var size = new Vector3(boundsSize.x * chunkSize.x * voxelSize, boundsSize.y * chunkSize.y * voxelSize,
                boundsSize.z * chunkSize.z * voxelSize);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                FromLocal((size / 2f) - (Vector3)chunkSize / 2f),
                size);
        }

        public Vector3 GetChunkPosition(int x, int y, int z)
        {
            return FromLocal(new Vector3(x * chunkSize.x, y * chunkSize.y, z * chunkSize.z) * voxelSize);
        }

        public ChunkVolume GetChunkByPos(Vector3Int chunkPos)
        {
            if (chunks.ContainsKey(chunkPos))
            {
                return chunks[chunkPos];
            }

            return null;
        }

        public void ReInitialize(Vector3Int bounds, Vector3Int chunk, float voxel)
        {
            foreach (var v in chunks)
            {
                Destroy(v.Value.gameObject);
            }

            foreach (var v in dynamicChunks)
            {
                Destroy(v.gameObject);
            }

            chunks.Clear();
            dynamicChunks.Clear();

            boundsSize = bounds;
            chunkSize = chunk;
            voxelSize = voxel;

            Init();
        }


        public void RemoveDynamicChunkVolume(DynamicChunkVolume dynamicChunkVolume)
        {
            dynamicChunks.Remove(dynamicChunkVolume);
            OnChunkRemoved?.Invoke(dynamicChunkVolume);
        }

        public DynamicChunkVolume CreateDynamicChunkVolume()
        {
            var chunk = spawnerFabric.SpawnItem(dynamicChunkVolumePrefab);
            dynamicChunks.Add(chunk);
            OnChunkAdded?.Invoke(chunk);
            return chunk;
        }

        public void SetChunksFromFile(SavedVolume targetVolumeData)
        {
            foreach (var chunk in targetVolumeData.chunks)
            {
                var spawnedChunk = GetChunkByPos(chunk.chunkPosition.Convert());
                if (spawnedChunk != null)
                {
                    SetBlocksToChunk(chunk, spawnedChunk);

                    spawnedChunk.ModifyChunk();
                }
            }
        }

        public void SetBlocksToChunk(ChunkData chunk, IMeshDrawable spawnedChunk)
        {
            for (var i = 0; i < chunk.blocksData.Length; i++)
            {
                var blockData = chunk.blocksData[i];
                for (int j = 0; j < blockData.lineLength; j++)
                {
                    spawnedChunk.BlocksData[blockData.id + j] = blockData.block;
                    if (blockData.block.type == 1)
                    {
                        spawnedChunk.BlocksData[blockData.id + j].health = materialListObject.GetMaterialHealth(blockData.block.materialId);
                    }
                }
            }
        }

        public void AddDynamicChunk(DynamicChunkVolume dynamicVolume)
        {
            dynamicChunks.Add(dynamicVolume);
            OnChunkAdded?.Invoke(dynamicVolume);
        }

        public void ModifyByNetChunksData(Dictionary<NetVector3Int,NetChunkData> blocksModuleStartChangedChunks)
        {
            foreach (var modChunk in blocksModuleStartChangedChunks)
            {
                var pos = modChunk.Key.Convert();
                if (chunks.ContainsKey(pos))
                {
                    foreach (var netBlockData in modChunk.Value.blocks)
                    {
                        if (netBlockData.Key >= 0 && netBlockData.Key < chunks[pos].BlocksData.Length)
                        {
                            chunks[pos].BlocksData[netBlockData.Key] = new Block()
                            {
                                type = (byte)netBlockData.Value.type,
                                materialId = (byte)netBlockData.Value.type
                            };
                        }
                    }

                }
            }
        }
    }
}
