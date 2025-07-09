using Content.Scripts.Game.IO;
using Content.Scripts.Game.Scriptable;
using Content.Scripts.Game.Services;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Content.Scripts.Game.Voxels
{
    public class DynamicChunkVolumeSaver : MonoBehaviour
    {
        [SerializeField] private string itemName;
        [SerializeField] private VoxelVolume volume;
        [SerializeField] private MaterialListObject materialListObject;
        VoxelVolumeIO voxelVolumeIO = new VoxelVolumeIO();


        [Button]
        private void SaveChunk()
        {
            voxelVolumeIO.PrepareSave(volume, materialListObject);
            voxelVolumeIO.SaveDataChunk(PathService.MapsPath + "\\" + itemName + ".vxchunk",
                GetComponent<DynamicChunkVolume>());
        }
    }
}
