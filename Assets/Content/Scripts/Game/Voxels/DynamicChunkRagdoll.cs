using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Content.Scripts.Game.Voxels
{
    [RequireComponent(typeof(DynamicChunkVolume))]
    public class DynamicChunkRagdoll : MonoBehaviour
    {
        [SerializeField] private Collider collider;
        private Rigidbody rb;

        [Inject]
        private void Construct()
        {
            rb = GetComponent<Rigidbody>();
            GetComponent<DynamicChunkVolume>().OnDynamicChunkChanged += delegate
            {
                rb.isKinematic = false;
            };
        }
        
        public Vector3 GetClosestPoint(Vector3 point)
        {
            return collider.ClosestPoint(point);
        }
        
        
        public void AddVelocity(Vector3 velocity, Vector3 pos)
        {
            rb.AddForceAtPosition(velocity, pos, ForceMode.Impulse);
            rb.AddRelativeTorque(Random.insideUnitSphere * Random.Range(-100, 100), ForceMode.Impulse);
        }
    }
}
