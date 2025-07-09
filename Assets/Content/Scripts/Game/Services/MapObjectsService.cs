using System.Collections;
using System.Collections.Generic;
using Content.Scripts.Game.Interfaces;
using Content.Scripts.Game.IO;
using Content.Scripts.Game.Scriptable;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game.Services
{
    public class MapObjectsService : MonoBehaviour
    {
        [SerializeField] private MapAdditionalListObject itemsList;

        private List<MapAdditionalItem> spawnedItems = new List<MapAdditionalItem>();
        private List<IPlayerSpawn> playerSpawns = new List<IPlayerSpawn>();
        private List<DynamicLight> dynamicLights = new List<DynamicLight>();
        private PrefabSpawnerFabric spawnerFabric;
        private DataLoaderService dataLoaderService;

        public List<IPlayerSpawn> PlayerSpawns => playerSpawns;

        public List<MapAdditionalItem> SpawnedItems => spawnedItems;


        [Inject]
        private void Construct(PrefabSpawnerFabric spawnerFabric, DataLoaderService dataLoaderService)
        {
            this.dataLoaderService = dataLoaderService;
            this.spawnerFabric = spawnerFabric;
        }



        public void LoadObjects(AdditionalObjectsData data)
        {
            foreach (MapAdditionalItem mapItem in spawnedItems)
            {
                Destroy(mapItem.gameObject);
            }

            playerSpawns.Clear();
            spawnedItems.Clear();
            dynamicLights.Clear();

            if (data != null)
            {
                for (var i = 0; i < data.Items.Count; i++)
                {
                    var it = spawnerFabric.SpawnItem(itemsList.GetItemByID(data.Items[i].ID), data.Items[i].Position,
                        Quaternion.Euler(data.Items[i].Rotation), transform);
                    it.transform.name += $"[{i}]";
                    spawnedItems.Add(it);
                    it.SetData(data.Items[i].GetDataDictionary());
                    GetPlayerSpawn(it);
                }

                foreach (var spawned in spawnedItems)
                {
                    spawned.Init(this);

                    var dynamicLight = spawned.GetComponent<DynamicLight>();
                    if (dynamicLight != null)
                    {
                        dynamicLight.Init((DynamicLight.LightBoundsList)spawned.GetKey("bounds"));
                        dynamicLights.Add(dynamicLight);
                    }
                }
            }
            
            StopAllCoroutines();
            StartCoroutine(LightsUpdateLoop());
        }

        IEnumerator LightsUpdateLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f);

                for (int i = 0; i < dynamicLights.Count; i++)
                {
                    dynamicLights[i].UpdateLight();
                }
            }
        }

        [Button]
        public void SaveObjectsDebug()
        {
            var io = new MapObjectsIO();
        
            io.PrepareSave(transform, itemsList);
            io.SaveData(dataLoaderService.LoadedMap.Path.DataFile);
        }
    

        private void GetPlayerSpawn(MapAdditionalItem it)
        {
            var playerSpawn = it.GetComponent<IPlayerSpawn>();
            if (playerSpawn != null)
            {
                playerSpawns.Add(playerSpawn);
            }
        }
    
    
    
    }
}
