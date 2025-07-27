using Content.Scripts.Game.Voxels;
using Content.Scripts.Services.Net;
using UnityEngine;
using UnityEngine.Serialization;

namespace Content.Scripts.Game.Weapons
{
    public class ProjectileRocket : ProjectileExplosive
    {
        [SerializeField] private GameObject particles;
        [SerializeField] private DebrisParticle debris;
        [SerializeField] private GameObject trail;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float speed = 10f;
        


        private float timer;

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
                var time = Vector3.Distance(startPos, endPos) / speed;
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
                rb.MovePosition(transform.position +  transform.forward * speed * Time.fixedDeltaTime);
            }
        }


        public override void Destroy()
        {
            if (isActive)
            {
                particles.transform.parent = null;
                particles.transform.up = startPos - transform.position;
                particles.gameObject.SetActive(true);
                var destroyed = voxelVolume.DestroyBlocksInRadius(transform.position, destroyData.Radius, (byte)destroyData.Damage);
                foreach (var keyValuePair in destroyed)
                {
                    var deb = prefabSpawnerFabric.SpawnItem(debris, transform.position, transform.rotation);
                    deb.Init((byte)keyValuePair.Key, keyValuePair.Value);
                }


                voxelVolume.ModifiedChunksDispose();
                voxelVolume.ModifiedNetChunksDispose(netService.GetModule<NetServiceBlocks>(), netObject.isMine);


                isActive = false;
                trail.transform.parent = null;
                Destroy(gameObject);//.SetActive(false);
                Destroy(trail.gameObject, 3);//.SetActive(false);
                Destroy(particles.gameObject, 3);//.SetActive(false);

                AddForceToPlayer();


                DestroyDynamicChunks();


                if (netObject.isMine)
                {
                    netService.GetModule<NetServiceProjectiles>().RPCDestroyProjectile(uid, transform.position);
                }
            }
        }

        





        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, destroyData.Radius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, forceRadius);
        }
    }
}