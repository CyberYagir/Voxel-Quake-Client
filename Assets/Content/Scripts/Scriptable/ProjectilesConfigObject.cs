using System.Collections.Generic;
using Content.Scripts.Game.Weapons;
using ServerLibrary.Structs;
using UnityEngine;

namespace Content.Scripts.Scriptable
{
    [CreateAssetMenu(menuName = "Create ProjectilesConfigObject", fileName = "ProjectilesConfigObject", order = 0)]
    public class ProjectilesConfigObject : ScriptableObject
    {
        [System.Serializable]
        public class Item
        {
            [SerializeField] private EProjectileType id;
            [SerializeField] private ProjectileBase projectile;

            public ProjectileBase Projectile => projectile;

            public EProjectileType ID => id;
        }

        [SerializeField] private List<Item> items = new List<Item>();

        public ProjectileBase GetProjectilePrefab(EProjectileType id)
        {
            return items.Find(x=>x.ID == id).Projectile;
        }
    }
}
