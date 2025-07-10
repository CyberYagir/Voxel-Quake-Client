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
                Instantiate(item, item.transform.parent)
                    .With(x => spawnedItems.Add(x))
                    .With(x => x.gameObject.SetActive(true));
            }
            inventory.OnInventoryChanged += Redraw;
            Redraw();
        }

        private void Redraw()
        {
            for (var i = 0; i < weaponsConfig.WeaponsList.Count; i++)
            {
                spawnedItems[i].Redraw(weaponsConfig.WeaponsList[i],
                    inventory.GetWeaponData(weaponsConfig.WeaponsList[i]));
            }
        }
    }
}
