using System.Collections.Generic;
using Content.Scripts.Game.Weapons;
using LightServer.Base.PlayersModule;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Content.Scripts.Scriptable
{
    [CreateAssetMenu(menuName = "Create WeaponDataObject", fileName = "WeaponDataObject", order = 0)]
    public class WeaponDataObject : ScriptableObject
    {
        [SerializeField] private EWeaponType type;
        [SerializeField] private WeaponBase prefab;
        [SerializeField, PreviewField] private Sprite icon;
        [SerializeField] private List<KeyCode> keys;
        [Space]
        [SerializeField] private int bullets;
        [SerializeField] private int bulletsAdd;
        [SerializeField] private int maxBullets;
        [SerializeField] private float fireRate;
        [SerializeField] private bool isDisplay;

        public float FireRate => fireRate;

        public int MaxBullets => maxBullets;

        public int Bullets => bullets;

        public Sprite Icon => icon;

        public WeaponBase Prefab => prefab;

        public int BulletsAdd => bulletsAdd;

        public List<KeyCode> Keys => keys;

        public bool IsDisplay => isDisplay;

        public EWeaponType Type => type;
    }
}
