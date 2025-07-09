using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Content.Scripts.Game.IO;
using Content.Scripts.Game.IO.Structures;
using Content.Scripts.Scriptable;
using Content.Scripts.Services.Net;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game.Services
{
    public class DataLoaderService : MonoBehaviour
    {
        [System.Serializable]
        public class MapPaths
        {
            [SerializeField] private string mapName;
            [SerializeField] private string mainFile;
            [SerializeField] private string dataFile;
            [SerializeField] private List<string> itemsPaths;

            public MapPaths(string mainFile, string dataFile, List<string> itemsPaths, string mapName)
            {
                this.mainFile = mainFile;
                this.dataFile = dataFile;
                this.itemsPaths = itemsPaths;
                this.mapName = mapName;
            }

            public List<string> ItemsPaths => itemsPaths;

            public string DataFile => dataFile;

            public string MainFile => mainFile;

            public string MapName => mapName;
        }
    
    
        [System.Serializable]
        public class MapData
        {
            private MapPaths path;
            private SavedVolume map;
            private AdditionalObjectsData data;

            private Dictionary<string, SavedVolume> items = new Dictionary<string, SavedVolume>();

            public MapData(SavedVolume map, AdditionalObjectsData data, Dictionary<string, SavedVolume> items, MapPaths path)
            {
                this.map = map;
                this.data = data;
                this.items = items;
                this.path = path;
            }

            public Dictionary<string, SavedVolume> Items => items;

            public AdditionalObjectsData Data => data;

            public SavedVolume Map => map;

            public MapPaths Path => path;
        }

        [SerializeField] private List<MapPaths> maps = new List<MapPaths>();
        private IFileInput<SavedVolume> voxelIO;
        private IFileInput<AdditionalObjectsData> mapObjectsIO;
    
        private MapData loadedMap;


        public bool isMapLoaded => loadedMap != null;

        public List<MapPaths> Maps => maps;

        public MapData LoadedMap => loadedMap;


        public event Action<MapData> OnMapLoaded;
    

        private NetServiceServer serverModule;
        private NetService netService;

        [Inject]
        private void Construct(NetService netService)
        {
            this.netService = netService;
            
            voxelIO = new VoxelVolumeIO();
            mapObjectsIO = new MapObjectsIO();


            
            PathService.CreateFolders();
            LoadMapsData();
            
            serverModule = netService.GetModule<NetServiceServer>();
            serverModule.LaunchAfterLoad(MapNameOnOnServerLoaded);
        }
        
        private void MapNameOnOnServerLoaded()
        {
            if (!LoadMap(serverModule.MapName))
            {
                netService.Disconnect($"Map not found [{serverModule}].");
            }
        }

        private void LoadMapsData()
        {
            var mapsFolders = Directory.GetDirectories(PathService.MapsPath);

            for (int i = 0; i < mapsFolders.Length; i++)
            {
                var mapName = Path.GetFileNameWithoutExtension(mapsFolders[i]);

                var mapFilePath = string.Empty;
                var dataFilePath = string.Empty;
            
                if (!GetFileInFolderByType(mapsFolders[i], "*.vx", out mapFilePath)) continue;
                if (!GetFileInFolderByType(mapsFolders[i], "*.vxobj", out dataFilePath)) continue;
            
                if (!Directory.Exists(mapsFolders[i] + "\\Items\\"))
                {
                    Directory.CreateDirectory(mapsFolders[i] + "\\Items\\");
                }

                List<string> items = Directory.GetFiles(mapsFolders[i] + "\\Items\\", "*.vxchunk").ToList();

                maps.Add(new MapPaths(mapFilePath, dataFilePath, items, mapName));
            }

            bool GetFileInFolderByType(string folder, string extension, out string mapFilePath)
            {
                var files = Directory.GetFiles(folder, extension);
                mapFilePath = string.Empty;
                if (files.Length > 0)
                {
                    mapFilePath = files[0];
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool LoadMap(string mapName)
        {
            var map = maps.Find(x => x.MapName.ToLower().Trim() == mapName.ToLower().Trim());

            if (map == null)
            {
                return false;
            }
            else
            {
                LoadMap(map);
                return true;
            }
        }

        public void LoadMap(MapPaths mapPaths)
        {
            var items = new Dictionary<string, SavedVolume>();
            for (int i = 0; i < mapPaths.ItemsPaths.Count; i++)
            {
                items.Add(Path.GetFileNameWithoutExtension(mapPaths.ItemsPaths[i]), voxelIO.LoadData(mapPaths.ItemsPaths[i]));
            }

            loadedMap = new MapData(voxelIO.LoadData(mapPaths.MainFile), mapObjectsIO.LoadData(mapPaths.DataFile), items, mapPaths);
            
            
            OnMapLoaded?.Invoke(loadedMap);
        }

        public SavedVolume GetItemData(string key)
        {
            if (isMapLoaded)
            {
                if (loadedMap.Items.ContainsKey(key))
                {
                    return loadedMap.Items[key];
                }
            }

            return default;
        }
    }
}
