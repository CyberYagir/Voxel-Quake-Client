using ServerLibrary.Structs;
using UnityEngine;

namespace Content.Scripts.Game.Weapons
{
    public class WeaponMachinegun : WeaponBase
    {
        [SerializeField] private float velAdd;
        [SerializeField] private float maxVel;
        [SerializeField] private float drag;
        [SerializeField] private Transform rotator;
        [SerializeField] private Transform jitterer;
        [SerializeField] private float spreadAngle = 5f;
        
        private float vel;

        
        public override void Shoot()
        {
            if (isCanShoot)
            {
                float randomX = Random.Range(-spreadAngle, spreadAngle);
                float randomY = Random.Range(-spreadAngle, spreadAngle);
                
                Vector3 spreadDirection = Quaternion.Euler(randomX, randomY, 0) * camera.transform.forward;
                
                
                if (Physics.Raycast(camera.transform.position, spreadDirection, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Default")))
                {
                    netServiceProjectiles.RPCSpawnProjectile(EProjectileType.Machinegun, camera.transform.position, spreadDirection, hit.point);
                }
                ResetTime();
            }
        }

        public override void UpdateWeaponPress(bool isDown, bool isHasBulelts)
        {
            base.UpdateWeaponPress(isDown, isHasBulelts);

            if (isDown && isHasBulelts)
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