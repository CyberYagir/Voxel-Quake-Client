using Content.Scripts.Game.Services;
using Content.Scripts.Scriptable;
using Content.Scripts.Services.Net;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Content.Scripts.Game.Weapons
{
    public abstract class WeaponBase : MonoBehaviour
    {
        [SerializeField] protected Transform projectileSpawnPoint;
        [SerializeField, ReadOnly] protected float time = 0;
        
        private float fireRate;
        protected PrefabSpawnerFabric spawnerFabric;
        protected Camera camera;
        protected NetServiceProjectiles netServiceProjectiles;

        public bool isCanShoot => time >= fireRate;
    
        public virtual void Init(PrefabSpawnerFabric spawnerFabric, Camera camera, NetServiceProjectiles netServiceProjectiles, WeaponDataObject weaponDataObject)
        {
            this.netServiceProjectiles = netServiceProjectiles;
            this.camera = camera;
            this.spawnerFabric = spawnerFabric;

            fireRate = weaponDataObject.FireRate;
            
            time = fireRate;
        }




        public virtual void UpdateWeapon()
        {
            time += Time.deltaTime;
        }
        
        public virtual void UpdateWeaponPress(bool isDown, bool isHaveBullets)
        {
            
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