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

        public virtual void SetHitScanPoint(Vector3 point, Vector3 dir)
        {
            
        }

        protected virtual void EndProjectile()
        {
            OnProjectileEnd?.Invoke();
            
        }
    }
}
