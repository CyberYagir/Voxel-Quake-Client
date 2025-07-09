using System.Collections;
using Content.Scripts.Game.Voxels;
using Content.Scripts.Services.Net;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game.Services
{
    public class MapLoaderService : MonoBehaviour
    {
        private DataLoaderService dataLoaderService;
        private VoxelVolume voxelVolume;
        private MapObjectsService mapObjectsService;
        private VoxelVolumeDrawer voxelVolumeDrawer;
        private NetServiceBlocks blocksModule;
        private NetServiceServer serverModule;

        [Inject]
        private void Construct(DataLoaderService dataLoaderService, VoxelVolume voxelVolume, MapObjectsService mapObjectsService, VoxelVolumeDrawer voxelVolumeDrawer, NetService netService)
        {
            this.voxelVolumeDrawer = voxelVolumeDrawer;
            this.mapObjectsService = mapObjectsService;
            this.voxelVolume = voxelVolume;
            this.dataLoaderService = dataLoaderService;
        
            blocksModule = netService.GetModule<NetServiceBlocks>();
            serverModule = netService.GetModule<NetServiceServer>();
            
            
            dataLoaderService.OnMapLoaded += OnMapLoaded;
        }

        private void OnMapLoaded(DataLoaderService.MapData data)
        {
            
            
            StartCoroutine(WaitVoxelLoading());

            IEnumerator WaitVoxelLoading()
            {
                while (!serverModule.IsLoaded)
                {
                    yield return null;
                }
                
                voxelVolume.ReInitialize(data.Map.boundsSize.Convert(), data.Map.chunkSize.Convert(), data.Map.voxelSize);
                voxelVolume.SetChunksFromFile(data.Map);
                voxelVolume.ModifyByNetChunksData(blocksModule.StartChangedChunks);
                
                while (voxelVolumeDrawer.IsMapGenerated)
                {
                    yield return null;
                }

                mapObjectsService.LoadObjects(data.Data);
            }
        }
    }
}
