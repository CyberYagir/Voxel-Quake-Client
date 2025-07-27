using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ServerLibrary.Structs;
using UnityEngine;

namespace Content.Scripts.Game.Weapons
{
    public class WeaponTribolt : WeaponBase
    {
        private static readonly int HASH_SHOOT_TRIGGER = Animator.StringToHash("Shoot");
        [SerializeField] private Animator animator;
        [SerializeField] private int burstCount;
        [SerializeField] private List<ParticleSystem> burstParticles;
        [SerializeField] private float shootDelay;


        public override void Shoot()
        {
            if (isCanShoot)
            {
                for (int i = 0; i < burstCount; i++)
                {
                    var id = i;
                    DOVirtual.DelayedCall(id * shootDelay, delegate
                    {
                        netServiceProjectiles.RPCSpawnProjectile(EProjectileType.Tribolt, camera.transform.position,
                            camera.transform.forward, projectileSpawnPoint.position, Vector3.zero);
                        
                        burstParticles[id].Play();
                    });
                }
                
                animator.SetTrigger(HASH_SHOOT_TRIGGER);
                
                ResetTime();
            }
        }
    }
}