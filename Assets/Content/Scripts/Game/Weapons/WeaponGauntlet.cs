using ServerLibrary.Structs;
using UnityEngine;

namespace Content.Scripts.Game.Weapons
{
    public class WeaponGauntlet : WeaponBase
    {
        [SerializeField] private float velAdd;
        [SerializeField] private float maxVel;
        [SerializeField] private float drag;
        [SerializeField] private Transform rotator;
        [SerializeField] private Transform jitterer;
        [SerializeField] private float maxDistance;
        
        private float vel;

        
        public override void Shoot()
        {
            if (isCanShoot)
            {
                if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, maxDistance, LayerMask.GetMask("Default")))
                {
                    netServiceProjectiles.RPCSpawnProjectile(EProjectileType.Gauntlet, camera.transform.position, camera.transform.forward, hit.point);
                }
                ResetTime();
            }
        }

        public override void UpdateWeaponPress(bool isDown)
        {
            base.UpdateWeaponPress(isDown);

            if (isDown)
            {
                vel += velAdd * Time.deltaTime;

                if (vel > maxVel)
                {
                    vel = maxVel;
                }
            }
            else
            {
                vel -= drag * Time.deltaTime;
                if (vel < 0)
                {
                    vel = 0;
                }
            }

            rotator.Rotate(Vector3.right * vel * Time.deltaTime, Space.Self);
            
            jitterer.localPosition = Vector3.Lerp(jitterer.localPosition, Random.insideUnitSphere * 0.01f * (vel/maxVel), vel * Time.deltaTime);
        }
    }
}