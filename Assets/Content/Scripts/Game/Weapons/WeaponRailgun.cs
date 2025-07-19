using Content.Scripts.Game.Services;
using Content.Scripts.Scriptable;
using Content.Scripts.Services.Net;
using DG.Tweening;
using ServerLibrary.Structs;
using UnityEngine;

namespace Content.Scripts.Game.Weapons
{
    public class WeaponRailgun : WeaponBase
    {
        private static readonly int HASH_SHOOT_TRIGGER = Animator.StringToHash("Shoot");
        private static readonly int HASH_EMISSION_ID = Shader.PropertyToID("_Emission");

        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private Animator animator;


        private Material material;
        private Color matColor;
        public override void Init(
            PrefabSpawnerFabric spawnerFabric, 
            Camera camera,
            NetServiceProjectiles netServiceProjectiles, 
            WeaponDataObject weaponDataObject)
        {
            base.Init(spawnerFabric, camera, netServiceProjectiles, weaponDataObject);

            var mats = meshRenderer.sharedMaterials;

            mats[1] = new Material(mats[1]);
            material = mats[1];
            matColor = material.GetColor(HASH_EMISSION_ID);
            meshRenderer.sharedMaterials = mats;
        }


        public void GlowReduce()
        {
            material.DOColor(Color.black, HASH_EMISSION_ID, 0.1f);
        }
        
        public void GlowAdd()
        {
            material.DOColor(matColor, HASH_EMISSION_ID, 1f).SetEase(Ease.InElastic);
        }

        public override void Shoot()
        {
            if (isCanShoot)
            {
                animator.SetTrigger(HASH_SHOOT_TRIGGER);
                muzzleFlash.Play(true);

                var endPoint = camera.transform.position + camera.transform.forward * 500;
                
                if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, 500,
                        LayerMask.GetMask("Default")))
                {
                    endPoint = hit.point;
                }

                netServiceProjectiles.RPCSpawnProjectile(EProjectileType.Rail, camera.transform.position, camera.transform.forward, projectileSpawnPoint.position, endPoint);
                ResetTime();
            }
        }
    }
}
