using ServerLibrary.Structs;
using UnityEngine;

namespace Content.Scripts.Game.Weapons
{
    public class WeaponShotgun : WeaponBase
    {
        private static readonly int HASH_SHOOT_TRIGGER = Animator.StringToHash("Shoot");
        [SerializeField] private ParticleSystem[] muzzleFlash;
        [SerializeField] private Animator animator;

        public override void Shoot()
        {
            if (isCanShoot)
            {
                netServiceProjectiles.RPCSpawnProjectile(EProjectileType.Shotgun, camera.transform.position,
                    camera.transform.forward, projectileSpawnPoint.position);
                
                animator.SetTrigger(HASH_SHOOT_TRIGGER);
                
                for (int i = 0; i < muzzleFlash.Length; i++)
                {
                    muzzleFlash[i].Play(true);
                }

                ResetTime();
            }
        }
    }
}