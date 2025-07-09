using System.Collections;
using System.Collections.Generic;
using Content.Scripts.Game.Voxels;
using Content.Scripts.Scriptable;
using Content.Scripts.Services;
using Content.Scripts.Services.Net;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game.Services
{
    public class PlayerService : MonoBehaviour
    {
        [SerializeField] private Camera menuCamera;
        [SerializeField] private FreeCameraController freeCamera;
        [SerializeField] private PlayerController player;
        [SerializeField, ReadOnly] private PlayerController spawnedPlayer;


        private Dictionary<int, PlayerController> spawnedPlayers = new Dictionary<int, PlayerController>();
        
        private VoxelVolumeDrawer volumeDrawer;
        private PrefabSpawnerFabric spawnerFabric;
        private MapObjectsService mapObjectsService;
        private NetService netService;
        private NetServicePlayers netPlayers;

        public PlayerController SpawnedPlayer => spawnedPlayer;


        [Inject]
        private void Construct(
            VoxelVolumeDrawer volumeDrawer, 
            PrefabSpawnerFabric spawnerFabric, 
            MapObjectsService mapObjectsService, 
            PlayerConfigObject playerConfig,
            NetService netService)
        {
            this.netService = netService;
            this.mapObjectsService = mapObjectsService;
            this.spawnerFabric = spawnerFabric;
            this.volumeDrawer = volumeDrawer;

            netPlayers = netService.GetModule<NetServicePlayers>();
            netPlayers.OnRespawnPlayer += CMDRespawnPlayerRecived;
            netPlayers.OnUpdatePlayerTransform += CMDUpdatePlayerTransform;
            netPlayers.OnDestroyPlayer += CMDPlayerDestroy;
  


            Application.targetFrameRate = playerConfig.GameFps;
            
            freeCamera.gameObject.SetActive(false);
            StartCoroutine(WaitForRespawn());

            volumeDrawer.OnMapGenerated += delegate
            {
                if (spawnedPlayer == null && !freeCamera.gameObject.activeInHierarchy)
                {
                    var spawn = GetRandomSpawn();
                    freeCamera.gameObject.SetActive(true);
                    freeCamera.Teleport(spawn);
                    menuCamera.enabled = false;
                }
            };
        }

        private void CMDPlayerDestroy(int ownerId)
        {
            if (spawnedPlayers.ContainsKey(ownerId))
            {
                Destroy(spawnedPlayers[ownerId].gameObject);
                spawnedPlayers.Remove(ownerId);
            }
        }

        private void CMDUpdatePlayerTransform(Vector3 pos, Vector3 rot, Vector3 vel, float cameraX, int ownerId)
        {
            if (spawnedPlayers.ContainsKey(ownerId))
            {
                spawnedPlayers[ownerId].NetController.SetNextTransform(pos, rot, vel, cameraX);
            }
        }
        private void CMDRespawnPlayerRecived(Vector3 pos, Vector3 rot, int ownerId)
        {
            var createdPlayer = netService.InstantiateNetObject<PlayerController>(player.gameObject, pos, rot, ownerId);
            spawnedPlayers.Add(ownerId, createdPlayer);
            
            if (netPlayers.LocalPlayerID == ownerId)
            {
                menuCamera.gameObject.SetActive(false);
                freeCamera.gameObject.SetActive(false);
                spawnedPlayer = createdPlayer;
            }
        }

        IEnumerator WaitForRespawn()
        {
            while (true)
            {
                if (volumeDrawer.IsMapGenerated)
                {
                    if (spawnedPlayer == null)
                    {
                        if (InputService.IsSpaceDown)
                        {
                            var spawn = GetRandomSpawn();
                            netPlayers.RespawnPlayerRPC(spawn.position, spawn.eulerAngles);
                            yield break;
                        }
                    }
                }

                yield return null;
            }
        }


        public void Respawn()
        {
       
            spawnedPlayer = spawnerFabric.SpawnItem(player);
        
            var spawn = GetRandomSpawn();
            spawnedPlayer.Teleport(spawn);
        }

        private Transform GetRandomSpawn()
        {
            if (mapObjectsService.PlayerSpawns.Count != 0)
            {
                return mapObjectsService.PlayerSpawns.GetRandomItem().Transform;
            }

            return transform;
        }

        public Vector3 GetPlayerPosition()
        {
            if (spawnedPlayer == null)
            {
                return freeCamera.transform.position;
            }
            
            return spawnedPlayer.transform.position;
        }
    }
}
