using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Content.Scripts.Game.Voxels
{
    public class VoxelFromModelParser : MonoBehaviour
    {
        [SerializeField] private VoxelVolume voxelVolume;

        [SerializeField] private bool drawPoses;
        [SerializeField] private bool pause;

        [Button]
        public void ParseObjects()
        {
            StartCoroutine(Loop());

        }

        IEnumerator Loop()
        {

            var startPos = voxelVolume.transform.position;

            var endPos = voxelVolume.transform.position +
                         new Vector3(
                             voxelVolume.BoundsSize.x * voxelVolume.ChunkSize.x * voxelVolume.VoxelSize,
                             voxelVolume.BoundsSize.y * voxelVolume.ChunkSize.y * voxelVolume.VoxelSize,
                             voxelVolume.BoundsSize.z * voxelVolume.ChunkSize.z * voxelVolume.VoxelSize
                         );

            for (float x = startPos.x; x < endPos.x; x += voxelVolume.VoxelSize/2f)
            {
                for (float y = startPos.y; y < endPos.y; y += voxelVolume.VoxelSize/2f)
                {
                    for (float z = startPos.z; z < endPos.z; z += voxelVolume.VoxelSize/2f)
                    {
                        var pos = new Vector3(x, y, z);
                        if (Physics.CheckBox(pos, Vector3.one * voxelVolume.VoxelSize,
                                Quaternion.identity))
                        {
                            voxelVolume.SetBlock(pos, 1, 1, out var chunk, false, null);
                        }  
                    }
                }
                yield return null;

                while (pause)
                {
                    voxelVolume.ModifiedChunksDispose();
                    yield return null; 
                }
            }


            yield return null;
        
            voxelVolume.ModifiedChunksDispose();
        }


    }
}
