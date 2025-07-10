using Content.Scripts.Scriptable;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Content.Scripts.Game.UI
{
    public class UIPlayerWeaponsItem : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text text;
        private WeaponDataObject weapon;

        public WeaponDataObject Weapon => weapon;


        public void Redraw(WeaponDataObject weapon, PlayerInventory.WeaponData weaponData)
        {
            this.weapon = weapon;
            icon.sprite = weapon.Icon;
            if (weaponData == null || weaponData.Bullets == 0)
            {
                icon.SetAlpha(0.35f);
                text.text = "";
            }
            else
            {
                icon.SetAlpha(1);
                text.text = weaponData.Bullets.ToString();
            }
        }
    }
}