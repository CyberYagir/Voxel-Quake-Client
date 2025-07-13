using Content.Scripts.Game.Voxels;
using Content.Scripts.Services.Net;
using UnityEngine;

namespace Content.Scripts.Game.Weapons
{
    public class ProjectileRocket : ProjectileBase
    {
        [SerializeField] private GameObject particles;
        [SerializeField] private DebrisParticle debris;
        [SerializeField] private GameObject trail;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float rocketSpeed = 10f;
        [SerializeField] private int rocketDamage = 10;
        [SerializeField] private float radius = 3f;
        [SerializeField] private float force;
        [SerializeField] private float forceRadius;

        private float timer;

        private bool isActive = true;
        private Vector3 endPos; 
        private Vector3 startPos; 
        public override void Init(Vector3 dir, int ownerID, string uid)
        {
            base.Init(dir, ownerID, uid);
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

            startPos = transform.position;
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 500, LayerMask.GetMask("Default")))
            {
                rb.isKinematic = true;
                endPos = hit.point;
            }
            else
            {
                rb.isKinematic = false;
            }
        }

        private void FixedUpdate()
        {

            Debug.DrawLine(startPos, endPos, Color.red);
            if (endPos != default)
            {
                var time = Vector3.Distance(startPos, endPos) / rocketSpeed;
                timer += Time.fixedDeltaTime;
                
                var percent = timer / time;

                transform.position = Vector3.Lerp(startPos, endPos, percent);

                if (percent >= 1f)
                {
                    transform.position = endPos;
                    if (netObject.isMine)
                    {
                        Destroy();
                    }
                }
            }
            else
            {
                rb.MovePosition(transform.position +  transform.forward * rocketSpeed * Time.fixedDeltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (netObject.isMine && !other.isTrigger)
            {
                Destroy();
            }
        }

        private void Destroy()
        {
            if (isActive)
            {
                particles.transform.parent = null;
                particles.transform.up = startPos - transform.position;
                particles.gameObject.SetActive(true);
                var destroyed = voxelVolume.DestroyBlocksInRadius(transform.position, radius, (byte)rocketDamage);
                foreach (var keyValuePair in destroyed)
                {
                    var deb = prefabSpawnerFabric.SpawnItem(debris, transform.position, transform.rotation);
                    deb.Init((byte)keyValuePair.Key, keyValuePair.Value);
                }


                voxelVolume.ModifiedChunksDispose();
                voxelVolume.ModifiedNetChunksDispose(netService.GetModule<NetServiceBlocks>(), netObject.isMine);


                isActive = false;
                trail.transform.parent = null;
                gameObject.SetActive(false);

                if (playerService.SpawnedPlayer != null)
                {
                    var closestPlayerPoint = playerService.SpawnedPlayer.ClosestPoint(transform.position);

                    var distance = Vector3.Distance(transform.position, closestPlayerPoint);

                    if (distance <= forceRadius)
                    {
                        var percent = 1 - (distance / forceRadius);

                        var targetForce = percent * force;

                        playerService.SpawnedPlayer.AddVelocity((closestPlayerPoint - transform.position).normalized *
                                                                targetForce);
                    }
                }


                for (var i = 0; i < voxelVolume.DynamicChunks.Count; i++)
                {
                    var ragdoll = voxelVolume.DynamicChunks[i].GetComponent<DynamicChunkRagdoll>();
                    if (ragdoll)
                    {
                        var closestPlayerPoint = ragdoll.GetClosestPoint(transform.position);

                        var distance = Vector3.Distance(transform.position, closestPlayerPoint);

                        if (distance <= radius)
                        {
                            var percent = 1 - (distance / forceRadius);

                            var targetForce = percent * force;

                            ragdoll.AddVelocity((closestPlayerPoint - transform.position).normalized * targetForce,
                                transform.position);
                        }
                    }
                }


                if (netObject.isMine)
                {
                    netService.GetModule<NetServiceProjectiles>().RPCDestroyProjectile(uid, transform.position);
                }
            }
        }

        public override void DestroyProjectile()
        {
            Destroy();
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, radius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, forceRadius);
        }
    }
}