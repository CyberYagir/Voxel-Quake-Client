using Content.Scripts.Game.Services;
using Content.Scripts.Services.Net;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Content.Scripts.Game.Weapons
{
    public abstract class WeaponBase : MonoBehaviour
    {
        [SerializeField] protected Transform projectileSpawnPoint;
        [SerializeField] private int bullets;
        [SerializeField] private int maxBullets;
        [SerializeField] private float fireRate;

    
        [SerializeField, ReadOnly] protected float time = 0;
        protected PrefabSpawnerFabric spawnerFabric;
        protected Camera camera;
        protected NetServiceProjectiles netServiceProjectiles;

        public bool isCanShoot => time >= fireRate;
    
        public virtual void Init(PrefabSpawnerFabric spawnerFabric, Camera camera, NetServiceProjectiles netServiceProjectiles)
        {
            this.netServiceProjectiles = netServiceProjectiles;
            this.camera = camera;
            this.spawnerFabric = spawnerFabric;
            time = fireRate;
        }




        public virtual void UpdateWeapon()
        {
            time += Time.deltaTime;
        }

        public virtual void ResetTime()
        {
            time = 0;
        }
    
    
        public virtual void Scope()
        {
        
        }
        public virtual void Shoot()
        {
        
        }
    
    }
}