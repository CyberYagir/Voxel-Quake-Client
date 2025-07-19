using ServerLibrary.Structs;
using UnityEngine;

namespace Content.Scripts.Game.Weapons
{
    public class WeaponRocketLauncher : WeaponBase
    {
        private static readonly int HASH_SHOOT_TRIGGER = Animator.StringToHash("Shoot");
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private Animator animator;
        public override void Shoot()
        {
            if (isCanShoot)
            {
                animator.SetTrigger(HASH_SHOOT_TRIGGER);
                muzzleFlash.Play(true);
                netServiceProjectiles.RPCSpawnProjectile(EProjectileType.Rocket, camera.transform.position, camera.transform.forward, projectileSpawnPoint.position,Vector3.zero);
                ResetTime();
            }
        }
    }
}