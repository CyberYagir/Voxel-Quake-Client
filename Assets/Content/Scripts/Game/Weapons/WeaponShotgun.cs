using System.Collections.Generic;
using ServerLibrary.Structs;
using UnityEngine;

namespace Content.Scripts.Game.Weapons
{
    public class WeaponShotgun : WeaponBase
    {
        private static readonly int HASH_SHOOT_TRIGGER = Animator.StringToHash("Shoot");
        [SerializeField] private ParticleSystem[] muzzleFlash;
        [SerializeField] private Animator animator;
        [SerializeField] private ProjectileShotgun projectileShotgun;

        public override void Shoot()
        {
            if (isCanShoot)
            {

                var pellets = CalculatePellets();

                foreach (var pellet in pellets)
                {
                    netServiceProjectiles.RPCSpawnProjectile(EProjectileType.Shotgun, camera.transform.position, camera.transform.forward, pellet, Vector3.zero);
                }

                animator.SetTrigger(HASH_SHOOT_TRIGGER);
                
                for (int i = 0; i < muzzleFlash.Length; i++)
                {
                    muzzleFlash[i].Play(true);
                }

                ResetTime();
            }
        }

        
        public List<Vector3> CalculatePellets()
        {
            float maxDistance = Mathf.Infinity;
            
            List<Vector3> pellets = new List<Vector3>();
            
            // Количество дробинок на каждом кольце
            int[] pelletsPerRing = { 1, 6, 12 }; // 1 в центре, 6 на среднем кольце, 12 на внешнем
            float[] ringRadii = { 0f, 0.06f, 0.12f }; // радиусы колец

            // Нормализуем направление и создаём плоскость перпендикулярную ему
            Vector3 point = camera.transform.position;
            Vector3 dir = camera.transform.forward;
            
            Vector3 forward = dir.normalized;
            Vector3 right = Vector3.Cross(forward, Vector3.up);
            if (right == Vector3.zero) right = Vector3.right; // если dir вертикальный
            Vector3 up = Vector3.Cross(right, forward);

            for (int ringIndex = 0; ringIndex < pelletsPerRing.Length; ringIndex++)
            {
                int pelletCount = pelletsPerRing[ringIndex];
                float radius = ringRadii[ringIndex];

                for (int i = 0; i < pelletCount; i++)
                {
                    Vector3 spreadDir;

                    if (radius == 0f)
                    {
                        // Центральный выстрел — прямо по направлению
                        spreadDir = forward;
                    }
                    else
                    {
                        // Расставляем пули равномерно по окружности
                        float angle = i * (360f / pelletCount) + ringIndex * 35f;
                        float radians = angle * Mathf.Deg2Rad;

                        float x = Mathf.Cos(radians) * radius;
                        float y = Mathf.Sin(radians) * radius;

                        Vector3 offset = right * x + up * y;
                        spreadDir = (forward + offset).normalized;
                    }

                    if (Physics.Raycast(point, spreadDir, out RaycastHit hit, maxDistance,
                            LayerMask.GetMask("Default")))
                    {
                        var pos = hit.point - ((hit.normal * projectileShotgun.RadiusData.Radius) / 2f);
                        pellets.Add(pos);
                    }
                }
            }

            return pellets;
        }
    }
}