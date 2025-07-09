using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Content.Scripts.Game.Voxels
{
    public class VoxelFromObjectsParser : MonoBehaviour
    {
        [SerializeField] private int blockType = 1;
        [SerializeField] private int materialID = 0;
        [SerializeField] private VoxelVolume voxelVolume;
    
        [SerializeField] private List<BoxCollider> boxColliders = new List<BoxCollider>();



        [Button]
        public void ParseObjects()
        {
            StartCoroutine(Wait());

        }


        [Button]
        public void Delete()
        {
            for (int i = 0; i < boxColliders.Count; i++)
            {
                if (boxColliders[i].transform.eulerAngles.magnitude <= 0.1f)
                {
                    DestroyImmediate(boxColliders[i].gameObject);
                }
                else
                {
                    print(boxColliders[i].transform.eulerAngles + " " + boxColliders[i].transform.name);
                }
            } 

            boxColliders.RemoveAll(x => x == null);
        }

        IEnumerator Wait()
        {
            for (int i = 0; i < boxColliders.Count; i++)
            {
                voxelVolume.AddBlocksFromBounds(boxColliders[i].transform, boxColliders[i].bounds, (byte)blockType, (byte)materialID);
                Debug.Log("Parse" + i);
                yield return null;
            }
            voxelVolume.ModifiedChunksDispose();
            gameObject.SetActive(false);
        }
    }
}
