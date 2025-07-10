using Content.Scripts.Game.Voxels;
using Content.Scripts.Services.Net;
using UnityEngine;

namespace Content.Scripts.Game.Weapons
{
    public class ProjectileGauntlet : ProjectileBase
    {
        [SerializeField] private ProjectileRail.RadiusData radiusData;
        [SerializeField] private DebrisParticle debris;

        private bool isActive = true;
        
        public override void Init(Vector3 dir, int ownerID, string uid)
        {
            base.Init(dir, ownerID, uid);
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            
            DestroyProjectile();
        }


        public override void DestroyProjectile()
        {
            if (isActive)
            {

                var destroyed = voxelVolume.DestroyBlocksInRadius(radiusData.Point.position, radiusData.Radius,
                    (byte)radiusData.Damage);
                foreach (var keyValuePair in destroyed)
                {
                    var deb = prefabSpawnerFabric.SpawnItem(debris, radiusData.Point.position,
                        radiusData.Point.rotation);
                    deb.Init((byte)keyValuePair.Key, keyValuePair.Value);
                }


                voxelVolume.ModifiedChunksDispose();
                voxelVolume.ModifiedNetChunksDispose(netService.GetModule<NetServiceBlocks>(), netObject.isMine);

                if (netObject.isMine)
                {
                    netService.GetModule<NetServiceProjectiles>().RPCDestroyProjectile(uid, transform.position);
                }

                gameObject.SetActive(false);
                isActive = false;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(radiusData.Point.position, radiusData.Radius);
        }
    }
}