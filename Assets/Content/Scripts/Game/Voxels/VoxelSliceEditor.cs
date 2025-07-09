using System.Collections;
using System.Collections.Generic;
using Content.Scripts.Game.Scriptable;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Content.Scripts.Game.Voxels
{
    public class VoxelSliceEditor : VolumeModification
    {
        [SerializeField] private IO.VoxelVolumeIO voxelVolumeIO;
        [SerializeField] private MaterialListObject materialListObject;
        [SerializeField] private List<Tilemap> tilemap;
        private Dictionary<int, Block[,]> blocksSlices = new Dictionary<int, Block[,]>();
        private Vector3Int voxelsCount;
        private VoxelVolume baseVolume;

        private void Awake()
        {
            
            // voxelVolumeIO.OnLoaded += VoxelVolumeIOOnOnLoaded;
        }

        private void VoxelVolumeIOOnOnLoaded()
        {
            blocksSlices.Clear();
            for (int y = 0; y < voxelsCount.y; y++)
            {
                var slice = new Block[voxelsCount.x, voxelsCount.z];
                int blockAdded = 0;
                for (int x = 0; x < voxelsCount.x; x++)
                {
                    for (int z = 0; z < voxelsCount.z; z++)
                    {
                        var blockIndex = baseVolume.GetBlockByVoxelPos(out var chunk, new Vector3Int(x, y, z));

                        if (blockIndex != -1)
                        {
                            slice[x, z] = chunk.BlocksData[blockIndex];
                            blockAdded++;
                        }
                    }
                }
                print("blocks in slice: " + blockAdded);

                blocksSlices.Add(y, slice);
            }
        }

        public override void Init(VoxelVolume baseVolume)
        {
            this.baseVolume = baseVolume;
            base.Init(baseVolume);


            voxelsCount = new Vector3Int(
                baseVolume.BoundsSize.x * baseVolume.ChunkSize.x,
                baseVolume.BoundsSize.y * baseVolume.ChunkSize.y, 
                baseVolume.BoundsSize.z * baseVolume.ChunkSize.z
            );


        }


        private List<int> drawedYs = new List<int>();
        [SerializeField] private int y;


        [Button]
        public void SaveDrawedToChunks()
        {
            Draw(y);
            StartCoroutine(Loop());
            IEnumerator Loop()
            {
                for (int y = 0; y < voxelsCount.y; y++)
                {
                    var slice = blocksSlices[y];
                    for (int x = 0; x < voxelsCount.x; x++)
                    {
                        for (int z = 0; z < voxelsCount.z; z++)
                        {
                            var blockIndex = baseVolume.GetBlockByVoxelPos(out var chunk, new Vector3Int(x, y, z));

                            if (blockIndex != -1)
                            {
                                chunk.BlocksData[blockIndex] = slice[x, z];
                                baseVolume.ModifyChunk(chunk);
                            }
                        }
                    }
                    
                    print($"{y}");
                    yield return null;
                }
                
                baseVolume.ModifiedChunksDispose();
            }
            
        }


        [Button]
        public void CopyTiles(Tilemap tileMapSource, Tilemap target)
        {
            for (int x = 0; x < voxelsCount.x; x++)
            {
                for (int z = 0; z < voxelsCount.z; z++)
                {
                    var pos = new Vector3Int(x, z, 0);
                    TileBase tile = tileMapSource.GetTile(pos);
                    target.SetTile(pos, tile);
                }
            }
            
            tileMapSource.RefreshAllTiles();
            target.RefreshAllTiles();
        }
        
        public void SaveDrawed()
        {
            for (int i = 0; i < drawedYs.Count; i++)
            {
                var map = tilemap[i];
                var y = drawedYs[i];
                
                var slice = blocksSlices[y];
                for (int x = 0; x < voxelsCount.x; x++)
                {
                    for (int z = 0; z < voxelsCount.z; z++)
                    {
                        var pos = new Vector3Int(x, z, 0);
                        if (map.HasTile(pos))
                        {
                            if (slice[x, z].type == 0)
                            {
                                slice[x, z].type = 1;
                            }
                            var tile = map.GetTile(pos);

                            var materialId = materialListObject.GetMaterialByTile(tile);
                            if (materialId != 255)
                            {
                                slice[x, z].materialId = materialId;
                            }
                        }
                        else
                        {
                            slice[x, z].type = 0;
                        }
            
                    }
                }
            }
            drawedYs.Clear();
        }

        [Button]
        public void Up()
        {
            y++;
            Draw(y);
        }
        [Button]
        public void Down()
        {
            y--;
            Draw(y);
        }
        
        [Button]
        public void Draw(int y)
        {
            this.y = y;
            SaveDrawed();
            
            for (var i = 0; i < tilemap.Count; i++)
            {
                tilemap[i].ClearAllTiles();
            }

            int tileID = 0;
            for (int i = -tilemap.Count; i < 0; i++)
            {
                DrawTileMap(y + i, tileID);
                tileID++;
            }
            
            
            for (var i = 0; i < tilemap.Count; i++)
            {
                tilemap[i].RefreshAllTiles();
            }
        }

        private void DrawTileMap(int y, int offset)
        {
            if (y >= 0 && y < blocksSlices.Count)
            {
                var slice = blocksSlices[y];
                for (int x = 0; x < voxelsCount.x; x++)
                {
                    for (int z = 0; z < voxelsCount.z; z++)
                    {
                        var pos = new Vector3Int(x, z, 0);
                        TileBase tile = null;
                        
                        if (slice[x, z].type != 0)
                        {
                            tile = materialListObject.GetVoxel((int)slice[x, z].materialId);
                        }

                        tilemap[offset].SetTile(pos, tile);
                        tilemap[offset].transform.position = new Vector3(0, y * baseVolume.VoxelSize, 0);
                    }
                }

                drawedYs.Add(y);
            }
        }
    }
}
