using System.Collections.Generic;
using Content.Scripts.Game.Services;
using Content.Scripts.Game.Weapons;
using Content.Scripts.Scriptable;
using Content.Scripts.Services.Net;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Content.Scripts.Game
{
    public partial class PlayerController
    {
        [System.Serializable]
        public class WeaponManager
        {
            [SerializeField] private Camera camera;
            [SerializeField] private Camera weaponCamera;
            [SerializeField] private Transform weaponHolder;
            [SerializeField] private Transform cameraLocalPosSwitch;
            [SerializeField] private AnimationCurve switchCurve;
            [SerializeField, ReadOnly] private bool isWeaponSwitch;
            
            private PrefabSpawnerFabric prefabSpawnerFabric;
            private NetServiceProjectiles netServiceProjectiles;
            private WeaponsConfigObject weaponsConfig;
            private PlayerInventory inventory;

            private WeaponDataObject selectedWeapon;
            private WeaponBase spawnedWeapon;
            public void Init(PrefabSpawnerFabric prefabSpawnerFabric, NetServiceProjectiles netServiceProjectiles, WeaponsConfigObject weaponsConfig, PlayerInventory inventory)
            {
                this.inventory = inventory;
                this.weaponsConfig = weaponsConfig;
                this.netServiceProjectiles = netServiceProjectiles;
                this.prefabSpawnerFabric = prefabSpawnerFabric;
            }

            private void ActiveWeapon(WeaponDataObject weaponDataObject)
            {
                if (selectedWeapon == weaponDataObject) return;


                selectedWeapon = weaponDataObject;
                
                isWeaponSwitch = true;

                var duration = 0.5f;
                var isSwitched = false;
                var startPos = weaponCamera.transform.localPosition;
                var startRot = weaponCamera.transform.localRotation;
                
                DOVirtual.Float(0, 1f, duration, delegate(float v)
                {
                    weaponCamera.transform.localPosition = Vector3.Lerp(startPos, cameraLocalPosSwitch.localPosition, v);
                    weaponCamera.transform.localRotation = Quaternion.Lerp(startRot, cameraLocalPosSwitch.localRotation, v);

                    if (v >= 0.5f && !isSwitched)
                    {
                        SpawnWeaponPrefab(weaponDataObject);

                        isSwitched = true;
                    }
                }).SetEase(switchCurve).onComplete += delegate
                {
                    isWeaponSwitch = false;
                };
            }

            private void SpawnWeaponPrefab(WeaponDataObject weaponDataObject)
            {
                if (spawnedWeapon != null)
                {
                    Destroy(spawnedWeapon.gameObject);
                }
                
                spawnedWeapon = Instantiate(weaponDataObject.Prefab, weaponHolder)
                    .With(x => x.Init(
                        prefabSpawnerFabric,
                        camera,
                        netServiceProjectiles,
                        weaponDataObject));
            }

            public void Update()
            {
                if (isWeaponSwitch) return;


                for (int i = 0; i < weaponsConfig.WeaponsList.Count; i++)
                {
                    if (inventory.HasWeapon(weaponsConfig.WeaponsList[i]))
                    {
                        for (int j = 0; j < weaponsConfig.WeaponsList[i].Keys.Count; j++)
                        {
                            if (Input.GetKeyDown(weaponsConfig.WeaponsList[i].Keys[j]))
                            {
                                ActiveWeapon(weaponsConfig.WeaponsList[i]);
                                return;
                            }
                        }
                    }
                }

                if (selectedWeapon != null)
                {
                     if (InputService.IsShootPressed)
                     {
                         if (spawnedWeapon.isCanShoot && inventory.HasBullets(selectedWeapon))
                         {
                             spawnedWeapon.Shoot();
                             inventory.RemoveBullet(selectedWeapon);
                         }
                     }
                     
                     spawnedWeapon.UpdateWeapon();
                }
            }
            
            public void DisableCamera()
            {
                camera.enabled = false;
            }
        }
    }
}