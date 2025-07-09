using Content.Scripts.Game.Services;
using Content.Scripts.Game.Voxels;
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

        public override void Init(MapObjectsService mapObjectsService)
        {
            base.Init(mapObjectsService);

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