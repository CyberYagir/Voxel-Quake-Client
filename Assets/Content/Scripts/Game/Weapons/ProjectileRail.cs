using System;
using System.Collections.Generic;
using Content.Scripts.Game.Voxels;
using Content.Scripts.Services.Net;
using DG.Tweening;
using UnityEngine;

namespace Content.Scripts.Game.Weapons
{
    public class ProjectileRail : ProjectileBase
    {
        [System.Serializable]
        public class RadiusData
        {
            [SerializeField] private float radius;
            [SerializeField] private float damage;
            [SerializeField] private Transform point;

            public Transform Point => point;

            public float Damage => damage;

            public float Radius => radius;
        }
        
        [SerializeField] private int damage = 70;

        [SerializeField] private LineRenderer line;
        [SerializeField] private List<RadiusData> damagePoints;
        [SerializeField] private Transform removeVoxelPoint;
        [SerializeField] private DebrisParticle debris;

        public override void Init(Vector3 dir, int ownerID, string uid)
        {
            base.Init(dir, ownerID, uid);
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            line.SetPosition(0, transform.position);

            DOVirtual.Float(line.widthMultiplier, 0, 4f, value =>
            {
                line.widthMultiplier = value;
            }).SetLink(gameObject).onComplete += EndProjectile;
        }

        protected override void EndProjectile()
        {
            base.EndProjectile();
            Destroy(gameObject);
        }

        public override void SetHitScanPoint(Vector3 point, Vector3 dir, Vector3 endPoint)
        {
            base.SetHitScanPoint(point, dir, endPoint);
            
            line.SetPosition(1, endPoint);
            
            removeVoxelPoint.transform.position = line.GetPosition(1);
            removeVoxelPoint.forward = line.GetPosition(1) - line.GetPosition(0);
            
            
            DestroyProjectile();
        }

        public override void DestroyProjectile()
        {
            foreach (RadiusData data in damagePoints)
            {
                var destroyed = voxelVolume.DestroyBlocksInRadius(data.Point.position, data.Radius, (byte)data.Damage);
                foreach (var keyValuePair in destroyed)
                {
                    var deb = prefabSpawnerFabric.SpawnItem(debris, data.Point.position, data.Point.rotation);
                    deb.Init((byte)keyValuePair.Key, keyValuePair.Value);
                }
            }
            voxelVolume.ModifiedChunksDispose();
            voxelVolume.ModifiedNetChunksDispose(netService.GetModule<NetServiceBlocks>(), netObject.isMine);
            
            if (netObject.isMine)
            {
                netService.GetModule<NetServiceProjectiles>().RPCDestroyProjectile(uid, transform.position);
            }
        }


        private void OnDrawGizmos()
        {
            for (int i = 0; i < damagePoints.Count; i++)
            {
                Gizmos.DrawWireSphere(damagePoints[i].Point.position, damagePoints[i].Radius);
            }
        }
    }
}
