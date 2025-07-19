using Content.Scripts.Game.Voxels;
using Content.Scripts.Services.Net;
using UnityEngine;

namespace Content.Scripts.Game.Weapons
{
    public class ProjectileShotgun : ProjectileBase
    {
        [SerializeField] private GameObject flashEffect;
        [SerializeField] private DebrisParticle debris;
        [SerializeField] private ProjectileRail.RadiusData radiusData;

        public ProjectileRail.RadiusData RadiusData => radiusData;
        
        public override void SetHitScanPoint(Vector3 point, Vector3 dir, Vector3 endPoint)
        {
            base.SetHitScanPoint(point, dir);

            transform.LookAt(point);
            
            var destroyed = voxelVolume.DestroyBlocksInRadius(transform.position, radiusData.Radius, (byte)radiusData.Damage);
            
            foreach (var keyValuePair in destroyed)
            {
                var deb = prefabSpawnerFabric.SpawnItem(debris, transform.position, transform.rotation);
                deb.Init((byte)keyValuePair.Key, keyValuePair.Value);
            }
            
            voxelVolume.ModifiedChunksDispose();
            voxelVolume.ModifiedNetChunksDispose(netService.GetModule<NetServiceBlocks>(), netObject.isMine);
            
            EndProjectile();
            DestroyProjectile();
        }

        public override void DestroyProjectile()
        {
            base.DestroyProjectile();
        }
    }
}