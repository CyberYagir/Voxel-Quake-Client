using UnityEngine;
using Zenject;

namespace Content.Scripts.Game.Voxels
{
    public class VoxelDestroyer : MonoBehaviour
    {
        [SerializeField] private float destructionRadius = 1f;
        [SerializeField] private Camera camera;
        private VoxelVolume targetVolume;

        [Inject]
        private void Construct(VoxelVolume voxelVolume)
        {
            targetVolume = voxelVolume;
        }
    
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                DestroyAtMousePosition();
            }
        }
    
        private void DestroyAtMousePosition()
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
         
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~0))
            {
                // targetVolume.DestroyBlocksInRadius(hit.point, destructionRadius, new List<int>() { 1 });
                targetVolume.ModifiedChunksDispose();
            }
        }
    }
}
