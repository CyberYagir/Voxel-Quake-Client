using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game.Voxels
{
    [RequireComponent(typeof(DynamicChunkVolume))]
    public class DynamicChunkObjectsDisabler : MonoBehaviour
    {
        [SerializeField] private List<GameObject> toDisable = new List<GameObject>();
        
        [Inject]
        private void Construct()
        {
            GetComponent<DynamicChunkVolume>().OnDynamicChunkChanged += OnOnDynamicChunkChanged;
        }

        private void OnOnDynamicChunkChanged()
        {
            for (var i = 0; i < toDisable.Count; i++)
            {
                toDisable[i].gameObject.SetActive(false);
            }
        }
    }
}
