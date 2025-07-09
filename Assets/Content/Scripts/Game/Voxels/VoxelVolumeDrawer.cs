using System;
using System.Collections;
using System.Collections.Generic;
using Content.Scripts.Game.Scriptable;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game.Voxels
{
    public class VolumeModification : MonoBehaviour
    {

        public virtual void Init(VoxelVolume baseVolume)
        {
        }
    }
    public class VoxelVolumeDrawer : VolumeModification
    {
        [SerializeField] private MaterialListObject materialListObject;
        private List<IMeshDrawable> toRedraw = new List<IMeshDrawable>(10);
        private int startChunksToLoad;
        private bool isMapGenerated = false;
    
        public event Action OnMapGenerated;
        public event Action OnMapGenerationStart;


        public int StartChunksToLoad => startChunksToLoad;
        public int ChunksToDraw => toRedraw.Count;

        public bool IsMapGenerated => isMapGenerated;

        [Inject]
        private void Construct(MaterialListObject materialListObject)
        {
            this.materialListObject = materialListObject;
        }
    
        public override void Init(VoxelVolume baseVolume)
        {
            OnMapGenerationStart?.Invoke();
            toRedraw.Clear();
            startChunksToLoad = baseVolume.Chunks.Count;
            baseVolume.OnChunkRemoved += OnChunkRemoved;
            baseVolume.OnChunkAdded += OnChunkAdded;
        
            foreach (var chunk in baseVolume.Chunks)
            {
                chunk.Value.OnChanged += AddChunk;
            }

            foreach (var chunk in baseVolume.DynamicChunks)
            {
                chunk.OnChanged += AddChunk;
            }

            StartCoroutine(AsyncDraw());
        }

        private void OnChunkAdded(IMeshDrawable obj)
        {
            obj.OnChanged += AddChunk;
        }

        private void OnChunkRemoved(IMeshDrawable obj)
        {
            toRedraw.Remove(obj);
        }


        private Mesh RedrawChunk(IMeshDrawable chunk)
        {
            var mesh = VoxelMeshGenerator.GenerateMesh(chunk, materialListObject);

            var modChunks = VoxelMeshGenerator.ModifiedNeighboursChunks;
        
            for (var i = 0; i < modChunks.Count; i++)
            {
                AddChunk(modChunks[i]);
            }

            return mesh;
        }

        private void AddChunk(IMeshDrawable obj)
        {
            if (toRedraw.Contains(obj))
            {
                toRedraw.Remove(obj);
            }
            
            toRedraw.Add(obj);
        }

        private Dictionary<IMeshDrawable, Mesh> modifiedMeshes = new Dictionary<IMeshDrawable, Mesh>(4096);


        IEnumerator AsyncDraw()
        {
            while (true)
            {
                if (toRedraw.Count != 0)
                {
                    int count = 0;
                    while (toRedraw.Count != 0)
                    {
                        var it = toRedraw[0];
                        toRedraw.RemoveAt(0);
                        if (!it.IsEmpty() || (it.IsEmpty() && it.HasMesh()))
                        {
                            modifiedMeshes.TryAdd(it, RedrawChunk(it));
                            count++;

                            if (count >= 4)
                            {
                                yield return null;
                                count = 0;
                            }
                        }
                    }


                    foreach (var k in modifiedMeshes)
                    {
                        k.Key.SetMesh(k.Value);
                    }

                    if (modifiedMeshes.Count != 0)
                    {
                        modifiedMeshes.Clear();
                        isMapGenerated = true;
                        OnMapGenerated?.Invoke();
                    }
                }

                yield return null;
            }
        }
    }
}