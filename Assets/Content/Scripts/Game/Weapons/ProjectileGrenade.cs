using System;
using Content.Scripts.Game.Voxels;
using Content.Scripts.Services.Net;
using UnityEngine;

namespace Content.Scripts.Game.Weapons
{
    public class ProjectileGrenade : ProjectileExplosive
    {
        private static Collider[] collidersCheck = new Collider[10];
        
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float startForce;
        [SerializeField] private float maxTime;
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private DebrisParticle debris;
        [SerializeField] private GameObject trail;
        [SerializeField] private SphereCollider sphereCollider;
        [SerializeField] private LayerMask explosionMask;

        private float timer;

        public override void Init(Vector3 dir, int ownerID, string uid)
        {
            base.Init(dir, ownerID, uid);
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

            rb.AddForce(transform.forward * startForce, ForceMode.Impulse);
        }


        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;

            if (maxTime <= timer)
            {
                Destroy();
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            rb.isKinematic = true;
        }

        public override void OnTriggered(Collider other)
        {
            rb.isKinematic = true;
        }

        public override void Destroy()
        {
            if (isActive)
            {
                particles.transform.parent = null;
                particles.transform.up = Vector3.up;
                particles.gameObject.SetActive(true);
                var destroyed = voxelVolume.DestroyBlocksInRadius(transform.position, destroyData.Radius,
                    (byte)destroyData.Damage);
                foreach (var keyValuePair in destroyed)
                {
                    var deb = prefabSpawnerFabric.SpawnItem(debris, transform.position, transform.rotation);
                    deb.Init((byte)keyValuePair.Key, keyValuePair.Value);
                }


                voxelVolume.ModifiedChunksDispose();
                voxelVolume.ModifiedNetChunksDispose(netService.GetModule<NetServiceBlocks>(), netObject.isMine);


                isActive = false;
                trail.transform.parent = null;
                Destroy(gameObject); //.SetActive(false);
                Destroy(trail.gameObject, 3); //.SetActive(false);
                Destroy(particles.gameObject, 3); //.SetActive(false);

                AddForceToPlayer();


                DestroyDynamicChunks();


                if (netObject.isMine)
                {
                    netService.GetModule<NetServiceProjectiles>().RPCDestroyProjectile(uid, transform.position);
                }
            }
        }
    }
}