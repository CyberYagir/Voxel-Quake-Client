using System;
using System.Collections.Generic;
using Content.Scripts.Scriptable;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Content.Scripts.Game
{
    public class PlayerInventory : MonoBehaviour
    {
        [System.Serializable]
        public class WeaponData
        {
            [SerializeField] private int bullets;
            [SerializeField] private WeaponDataObject weaponDataObject;

            public WeaponData(WeaponDataObject weaponDataObject)
            {
                this.weaponDataObject = weaponDataObject;

                bullets = weaponDataObject.Bullets;
            }

            public WeaponDataObject WeaponDataObject => weaponDataObject;

            public int Bullets => bullets;

            public void AddBullets()
            {
                if (bullets == 0)
                {
                    bullets = weaponDataObject.Bullets;
                }
                else
                {
                    bullets += weaponDataObject.BulletsAdd;
                    if (bullets > weaponDataObject.MaxBullets)
                    {
                        bullets = weaponDataObject.MaxBullets;
                    }
                }
            }

            public void RemoveBullet()
            {
                bullets--;
            }
        }
        
        [SerializeField] private List<WeaponData> inventory;
        [SerializeField] private List<WeaponDataObject> startWeapons;
        public event Action OnInventoryChanged;
        
        public void Init()
        {
            for (int i = 0; i < startWeapons.Count; i++)
            {
                AddWeapon(startWeapons[i]);
            }
        }
        
        [Button]
        public void AddWeapon(WeaponDataObject weaponDataObject)
        {
            var weaponData = inventory.Find(x => x.WeaponDataObject == weaponDataObject);
            if (weaponData == null)
            {
                weaponData = new WeaponData(weaponDataObject);
                inventory.Add(weaponData);
                OnInventoryChanged?.Invoke();
                return;
            }

            weaponData.AddBullets();
            
            OnInventoryChanged?.Invoke();
        }

        public void RemoveBullet(WeaponDataObject weaponDataObject)
        {
            var weaponData = inventory.Find(x => x.WeaponDataObject == weaponDataObject);

            if (weaponData != null && weaponData.Bullets != -1)
            {
                weaponData.RemoveBullet();
            }
            OnInventoryChanged?.Invoke();
        }


        public bool HasWeapon(WeaponDataObject weapon)
        {
            var weaponData = inventory.Find(x => x.WeaponDataObject == weapon);

            return weaponData != null;
        }

        public bool HasBullets(WeaponDataObject weapon)
        {
            var weaponData = inventory.Find(x => x.WeaponDataObject == weapon);

            if (weaponData != null)
            {
                if (weaponData.Bullets == -1) return true;
                
                return weaponData.Bullets > 0;
            }

            return false;
        }

        public WeaponData GetWeaponData(WeaponDataObject weapon)
        {
            var weaponData = inventory.Find(x => x.WeaponDataObject == weapon);

            return weaponData;
        }

        public WeaponDataObject GetAnyActiveWeapon()
        {
            var weapon = inventory.Find(x => x.Bullets > 0);

            if (weapon == null)
            {
                weapon = inventory.Find(x => x.Bullets == -1);
            }

            return weapon.WeaponDataObject;
        }
    }
}
