using System.Collections.Generic;
using Content.Scripts.Game.Services;
using Content.Scripts.Game.Voxels;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game.MapObjects
{
    public class MapItemLayerChanger : MapAdditionalItem
    {

        [System.Serializable]
        public class ChunksList
        {
            [SerializeField] private List<Vector3Int> chunksIDs = new List<Vector3Int>();

            public List<Vector3Int> ChunksIDs
            {
                get => chunksIDs;
                set => chunksIDs = value;
            }
        }
        
        [SerializeField, ValueDropdown(nameof(GetLayersNames))] private int layer;
        
        
        private VoxelVolume voxelVolume;





        [Inject]
        private void Construct(VoxelVolume voxelVolume)
        {
            this.voxelVolume = voxelVolume;
        }

        public override void Init(MapObjectsService mapObjectsService)
        {
            base.Init(mapObjectsService);
            
            var chunks = (ChunksList)GetKey("layers");

            for (int i = 0; i < chunks.ChunksIDs.Count; i++)
            {
                voxelVolume.GetChunkByPos(chunks.ChunksIDs[i]).ChangeLayer(layer);
            }
        }
        private IEnumerable<ValueDropdownItem<int>> GetLayersNames() => Extensions.GetNamedLayers();
    }
}
