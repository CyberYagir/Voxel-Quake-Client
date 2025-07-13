using Content.Scripts.Game.Voxels;
using Content.Scripts.Services.Net;
using DG.Tweening;
using UnityEngine;

namespace Content.Scripts.Game.Weapons
{
    public class ProjectileBullet : ProjectileBase
    {
        [SerializeField] private ProjectileRail.RadiusData radiusData;
        [SerializeField] private DebrisParticle debris;
        public override void Init(Vector3 dir, int ownerID, string uid)
        {
            base.Init(dir, ownerID, uid);
            DOVirtual.DelayedCall(3f, EndProjectile).SetLink(gameObject).SetUpdate(UpdateType.Fixed);
            OnProjectileEnd += () =>
            {
                Destroy(gameObject);
            };
        }

        public override void SetHitScanPoint(Vector3 point, Vector3 dir)
        {
            base.SetHitScanPoint(point, dir);
            if (Physics.Raycast(point, dir, out RaycastHit hit, 500, LayerMask.GetMask("Default")))
            {
                transform.position = hit.point;
                transform.rotation = Quaternion.LookRotation(-dir);
            }
            else
            {
                gameObject.SetActive(false);
            }
            
            
            DestroyProjectile();
        }

        public override void DestroyProjectile()
        {
            
            var destroyed = voxelVolume.DestroyBlocksInRadius(radiusData.Point.position, radiusData.Radius, (byte)radiusData.Damage);
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
        }

    }
}