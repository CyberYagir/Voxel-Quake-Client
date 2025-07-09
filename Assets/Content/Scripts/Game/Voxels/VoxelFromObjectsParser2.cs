using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Content.Scripts.Game.Scriptable;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Content.Scripts.Game.Voxels
{
    public class VoxelFromObjectsParser2 : MonoBehaviour
    {
    
        [SerializeField] private VoxelVolume voxelVolume;
        [SerializeField] private MaterialListObject materialListObject;
        [SerializeField] private List<BoxCollider> boxColliders = new List<BoxCollider>();

        private Dictionary<BoxCollider, byte> voxelDataObjects = new Dictionary<BoxCollider, byte>();


        [Button]
        public void ParseObjects()
        {
            GetColliders();
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

        [Button]
        public void GetColliders()
        {
            boxColliders = GetComponentsInChildren<BoxCollider>().ToList();
            voxelDataObjects.Clear();
            foreach (BoxCollider boxCollider in boxColliders)
            {
                var mat = boxCollider.GetComponent<Renderer>().sharedMaterial;
                voxelDataObjects.Add(boxCollider, materialListObject.GetVoxelByMaterial(mat));

            }

            boxColliders = boxColliders.OrderBy(x => materialListObject.GetVoxelByMaterial(voxelDataObjects[x]).Health)
                .ToList();
        }

        IEnumerator Wait()
        {
            for (int i = 0; i < boxColliders.Count; i++)
            {
                var mat = voxelDataObjects[boxColliders[i]];
                voxelVolume.AddBlocksFromBounds(boxColliders[i].transform, boxColliders[i].bounds, 1, mat);
                Debug.Log("Parse" + i);
                yield return null;
            }

            var brushes = GetComponentsInChildren<VoxelPlaneAdder>();
        
            foreach (var voxelFromObjectsParser2 in brushes)
            {
                voxelFromObjectsParser2.Active();
            }
        
            voxelVolume.ModifiedChunksDispose();
            gameObject.SetActive(false);
        
        
        }
    }
}
