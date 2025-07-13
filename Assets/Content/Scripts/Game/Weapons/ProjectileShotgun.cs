using Content.Scripts.Game.Voxels;
using Content.Scripts.Services.Net;
using UnityEngine;

namespace Content.Scripts.Game.Weapons
{
    public class ProjectileShotgun : ProjectileBase
    {
        [SerializeField] private GameObject flashEffect;
        [SerializeField] private DebrisParticle debris;
        [SerializeField] private ProjectileRail.RadiusData radiusData;


        public override void Init(Vector3 dir, int ownerID, string uid)
        {
            base.Init(dir, ownerID, uid);
        }
        public override void SetHitScanPoint(Vector3 point, Vector3 dir)
        {
            base.SetHitScanPoint(point, dir);

            float maxDistance = Mathf.Infinity;

            // Количество дробинок на каждом кольце
            int[] pelletsPerRing = { 1, 6, 12 }; // 1 в центре, 6 на среднем кольце, 12 на внешнем
            float[] ringRadii = { 0f, 0.06f, 0.12f }; // радиусы колец

            // Нормализуем направление и создаём плоскость перпендикулярную ему
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

                    if (Physics.Raycast(point, spreadDir, out RaycastHit hit, maxDistance, LayerMask.GetMask("Default")))
                    {
                        Instantiate(flashEffect, hit.point, Quaternion.LookRotation(point-hit.point))
                            .With(x=>x.gameObject.SetActive(true));
                        
                        var destroyed = voxelVolume.DestroyBlocksInRadius(hit.point - ((hit.normal * radiusData.Radius)/2f), radiusData.Radius, (byte)radiusData.Damage);
                        foreach (var keyValuePair in destroyed)
                        {
                            var deb = prefabSpawnerFabric.SpawnItem(debris, transform.position, transform.rotation);
                            deb.Init((byte)keyValuePair.Key, keyValuePair.Value);
                        }
                    }
                }
            }
            
            voxelVolume.ModifiedChunksDispose();
            voxelVolume.ModifiedNetChunksDispose(netService.GetModule<NetServiceBlocks>(), netObject.isMine);
            
            EndProjectile();
            DestroyProjectile();
        }

        public override void DestroyProjectile()
        {
            base.DestroyProjectile();
        }
    }
}