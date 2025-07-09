using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Content.Scripts.Game.Scriptable
{
    [CreateAssetMenu(fileName = "MaterialList", menuName = "Voxel/Material List")]
    public class MaterialListObject : ScriptableObject
    {
        [SerializeField]
        private List<VoxelDataObject> materials = new List<VoxelDataObject>();
    
        // Кэш для быстрого доступа по ID
        private Dictionary<int, VoxelDataObject> materialCache;
        private Dictionary<TileBase, int> tileCache;
        private bool cacheInitialized = false;
    
        public Material GetMaterial(int materialId)
        {
            if (!cacheInitialized)
            {
                InitializeCache();
            }

            if (materialCache.TryGetValue(materialId, out VoxelDataObject voxel))
            {
                return voxel.Material;
            }

            // Возвращаем дефолтный материал если не найден
            Debug.LogWarning($"Material with ID {materialId} not found. Using default material.");
            return GetDefaultMaterial();
        }
    
        public byte GetMaterialByTile(TileBase tile)
        {
            if (tileCache.TryGetValue(tile, out int materialId))
            {
                return (byte)materialId;
            }

            return 255;
        }
    
        public byte GetVoxelByMaterial(Material mat)
        {
            if (!cacheInitialized)
            {
                InitializeCache();
            }

            var vox = materials.FindIndex(x => x.Material == mat);

            return (byte)vox;
        }
    
        public VoxelDataObject GetVoxelByMaterial(byte id)
        {
            if (!cacheInitialized)
            {
                InitializeCache();
            }

            var vox = materials[id];
            return vox;
        }
    
        public TileBase GetVoxel(int materialId)
        {
            if (!cacheInitialized)
            {
                InitializeCache();
            }

            if (materialCache.TryGetValue(materialId, out VoxelDataObject voxel))
            {
                return voxel.Tile;
            }
        
            return null;
        }
    
        private void InitializeCache()
        {
            materialCache = new Dictionary<int, VoxelDataObject>();
            tileCache = new Dictionary<TileBase, int>();
            int id = 0;
            foreach (var materialData in materials)
            {
                if (materialData.Material != null && !materialCache.ContainsKey(id))
                {
                    materialCache[id] = materialData;
                    tileCache.Add(materialData.Tile, id);
                }
            

                id++;
            }
        
            cacheInitialized = true;
        }
        private Material GetDefaultMaterial()
        {
            if (materials.Count > 0 && materials[0].Material != null)
            {
                return materials[0].Material;
            }
        
            // Создаем простой дефолтный материал если ничего нет
            return new Material(Shader.Find("Standard"));
        }
    
        // Вызывается при загрузке ScriptableObject
        private void OnEnable()
        {
            cacheInitialized = false;
        }


        public int GetMaterialHealth(byte blockMaterialId)
        {
            if (!cacheInitialized)
            {
                InitializeCache();
            }

            return GetVoxelByMaterial(blockMaterialId).Health;
        }
    }
}