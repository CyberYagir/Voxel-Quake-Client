using System;
using Content.Scripts.Game.Services;
using Content.Scripts.Game.Voxels;
using Content.Scripts.Services.Net;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game.Weapons
{
    public abstract class ProjectileBase : MonoBehaviour
    {
        [SerializeField] protected NetObject netObject;

        public event Action OnProjectileEnd;
        
        protected VoxelVolume voxelVolume;
        protected PrefabSpawnerFabric prefabSpawnerFabric;
        protected PlayerService playerService;
        protected NetService netService;
        protected string uid;

        [Inject]
        private void Construct(VoxelVolume voxelVolume, PrefabSpawnerFabric prefabSpawnerFabric, PlayerService playerService, NetService netService)
        {
            this.netService = netService;
            this.playerService = playerService;
            this.prefabSpawnerFabric = prefabSpawnerFabric;
            this.voxelVolume = voxelVolume;
        }

        public virtual void Init(Vector3 dir, int ownerID, string uid)
        {
            this.uid = uid;
            netObject.Init(netService.Peer.RemoteId, ownerID);
        }

        public virtual void DestroyProjectile()
        {
            
        }

        public virtual void SetHitScanPoint(Vector3 point, Vector3 dir, Vector3 endPoint = default)
        {
            
        }

        protected virtual void EndProjectile()
        {
            OnProjectileEnd?.Invoke();
            
        }
    }

    public abstract class ProjectileExplosive : ProjectileBase
    {
        [SerializeField] protected float force;
        [SerializeField] protected float forceRadius;
        [SerializeField] protected ProjectileRail.RadiusData destroyData;

        protected bool isActive = true;
        
        public override void DestroyProjectile()
        {
            Destroy();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (netObject.isMine && !other.isTrigger)
            {
                OnTriggered(other);
            }
        }

        public virtual void OnTriggered(Collider other)
        {
            Destroy();
        }

        public virtual void Destroy()
        {
            
        }
        
        protected void AddForceToPlayer()
        {
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
        }
        
        protected void DestroyDynamicChunks()
        {
            for (var i = 0; i < voxelVolume.DynamicChunks.Count; i++)
            {
                var ragdoll = voxelVolume.DynamicChunks[i].GetComponent<DynamicChunkRagdoll>();
                if (ragdoll)
                {
                    var closestPlayerPoint = ragdoll.GetClosestPoint(transform.position);

                    var distance = Vector3.Distance(transform.position, closestPlayerPoint);

                    if (distance <= destroyData.Radius)
                    {
                        var percent = 1 - (distance / forceRadius);

                        var targetForce = percent * force;

                        ragdoll.AddVelocity((closestPlayerPoint - transform.position).normalized * targetForce,
                            transform.position);
                    }
                }
            }
        }

    }
}
