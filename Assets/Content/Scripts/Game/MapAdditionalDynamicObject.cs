using Content.Scripts.Game.Services;
using Content.Scripts.Game.Voxels;
using Content.Scripts.Services.Net;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game
{
    public class MapAdditionalDynamicObject: MapAdditionalItem
    {
        [SerializeField] private DynamicChunkVolume dynamicVolume;
        private DataLoaderService dataLoaderService;
        private VoxelVolume voxelVolume;

        [Inject]
        private void Construct(DataLoaderService dataLoaderService, VoxelVolume voxelVolume)
        {
            this.voxelVolume = voxelVolume;
            this.dataLoaderService = dataLoaderService;
        }

        public override void Init(MapObjectsService mapObjectsService, int uid, NetService netService)
        {
            base.Init(mapObjectsService, uid, netService);

            var key = (string)GetKey("item_name");

            if (key != null)
            {
                var itemVoxData = dataLoaderService.GetItemData(key);
                voxelVolume.AddDynamicChunk(dynamicVolume);
                dynamicVolume.LoadBlocks(itemVoxData);
            }
        }
    }
}