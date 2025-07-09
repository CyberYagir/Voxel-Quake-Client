using System.Collections.Generic;
using System.IO;
using Content.Scripts.Game.IO.Structures;
using Content.Scripts.Game.Scriptable;
using Content.Scripts.Game.Voxels;
using UnityEngine;

namespace Content.Scripts.Game.IO
{
    public partial class VoxelVolumeIO : IFileInput<SavedVolume>, IFileOutput
    {
        // [Button]
        // public void SaveToFile()
        // {
        //     var nonEmptyChunks = new List<ChunkData>();
        //
        //     foreach (var voxelVolumeChunk in voxelVolume.Chunks)
        //     {
        //         if (!voxelVolumeChunk.Value.IsEmpty())
        //         {
        //             nonEmptyChunks.Add(new ChunkData(voxelVolumeChunk.Value.ChunkPos, voxelVolumeChunk.Value.BlocksData));
        //         }
        //     }
        //
        //     targetVolumeData = new SavedVolume(voxelVolume.BoundsSize, voxelVolume.ChunkSize, voxelVolume.VoxelSize)
        //     {
        //         chunks = nonEmptyChunks.ToArray()
        //     };
        //
        //     // Используем бинарную сериализацию с Gzip сжатием
        //     // SaveWithCompression(targetVolumeData, fileName);
        // }

        // [Button]
        // public void SaveToFileChunk(DynamicChunkVolume chunk)
        // {
        //     targetVolumeData = new SavedVolume(voxelVolume.BoundsSize, voxelVolume.ChunkSize, voxelVolume.VoxelSize)
        //     {
        //         chunks = new[] { new ChunkData(Vector3Int.zero, chunk.BlocksData) }
        //     };
        //
        //     SaveWithCompression(targetVolumeData, fileName);
        // }
        //
        //
        //
        // public void LoadAsync(string path)
        // {
        //     SetPath(path);
        //     OnLoadStart?.Invoke();
        //     StartCoroutine(LoadFrameSkip());
        //     IEnumerator LoadFrameSkip()
        //     {
        //         yield return null;
        //         
        //         LoadFromFile();
        //     }
        // }
        //
        //
        // [Button]
        // public void LoadFromFile()
        // {
        //     targetVolumeData = LoadWithCompression(fileName);
        //     
        //     voxelVolume.ReInitialize(targetVolumeData.boundsSize.Convert(), targetVolumeData.chunkSize.Convert(), targetVolumeData.voxelSize);
        //
        //     foreach (var chunk in targetVolumeData.chunks)
        //     {
        //         var spawnedChunk = voxelVolume.GetChunkByPos(chunk.chunkPosition.Convert());
        //
        //         if (spawnedChunk != null)
        //         {
        //             for (var i = 0; i < chunk.blocksData.Length; i++)
        //             {
        //                 var blockData = chunk.blocksData[i];
        //                 for (int j = 0; j < blockData.lineLength; j++)
        //                 {
        //                     spawnedChunk.BlocksData[blockData.id + j] = blockData.block;
        //                     if (blockData.block.type == 1)
        //                     {
        //                         spawnedChunk.BlocksData[blockData.id + j].health = materialListObject.GetMaterialHealth(blockData.block.materialId);
        //                     }
        //                 }
        //             }
        //             
        //             spawnedChunk.ModifyChunk();
        //         }
        //     }
        //     
        //     OnLoaded?.Invoke();
        // }

        public SavedVolume LoadData(string path)
        {
            if (File.Exists(path))
            {
                return LoadWithCompression(path);
            }
            else
            {
                return default;
            }
        }


        private MaterialListObject materialListObject;
        private VoxelVolume voxelVolume;

        public void PrepareSave(VoxelVolume volume, MaterialListObject materialListObject)
        {
            this.voxelVolume = volume;
            this.materialListObject = materialListObject;
        }

        public void SaveData(string path)
        {
            if (!voxelVolume) return;
            if (!materialListObject) return;


            var nonEmptyChunks = new List<ChunkData>();

            foreach (var voxelVolumeChunk in voxelVolume.Chunks)
            {
                if (!voxelVolumeChunk.Value.IsEmpty())
                {
                    nonEmptyChunks.Add(
                        new ChunkData(voxelVolumeChunk.Value.ChunkPos, voxelVolumeChunk.Value.BlocksData));
                }
            }

            var targetVolumeData = new SavedVolume(voxelVolume.BoundsSize, voxelVolume.ChunkSize, voxelVolume.VoxelSize)
            {
                chunks = nonEmptyChunks.ToArray()
            };

            SaveWithCompression(targetVolumeData, path);
        }

        public void SaveDataChunk(string path, IMeshDrawable chunk)
        {
            if (!voxelVolume) return;
            if (!materialListObject) return;


            var nonEmptyChunks = new List<ChunkData>();

            nonEmptyChunks.Add(new ChunkData(Vector3Int.zero, chunk.BlocksData));

            var targetVolumeData = new SavedVolume(Vector3Int.zero, chunk.ChunkSize, chunk.VoxelSize)
            {
                chunks = nonEmptyChunks.ToArray()
            };

            SaveWithCompression(targetVolumeData, path);
        }
    }
}