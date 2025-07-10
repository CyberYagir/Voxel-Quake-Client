using System.Collections.Generic;
using Content.Scripts.Scriptable;
using UnityEngine;

namespace Content.Scripts.Game.UI
{
    public class UIPlayerWeapons : MonoBehaviour
    {
        [SerializeField] private UIPlayerWeaponsItem item;

        private List<UIPlayerWeaponsItem> spawnedItems = new List<UIPlayerWeaponsItem>();
        
        private WeaponsConfigObject weaponsConfig;
        private PlayerInventory inventory;

        public void Init(PlayerInventory inventory, WeaponsConfigObject weaponsConfig)
        {
            this.inventory = inventory;
            this.weaponsConfig = weaponsConfig;
            item.gameObject.SetActive(false);

            for (var i = 0; i < weaponsConfig.WeaponsList.Count; i++)
            {
                if (weaponsConfig.WeaponsList[i].IsDisplay)
                {
                    var id = i;
                    Instantiate(item, item.transform.parent)
                        .With(x => spawnedItems.Add(x))
                        .With(x=>x.Redraw(weaponsConfig.WeaponsList[id], inventory.GetWeaponData(weaponsConfig.WeaponsList[id])))
                        .With(x => x.gameObject.SetActive(true));
                }
            }

            inventory.OnInventoryChanged += Redraw;
            Redraw();
        }

        private void Redraw()
        {
            for (var i = 0; i < spawnedItems.Count; i++)
            {
                var weapon = spawnedItems[i].Weapon;
                spawnedItems[i].Redraw(weapon, inventory.GetWeaponData(weapon));
            }
        }
    }
}
