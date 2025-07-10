using Content.Scripts.Game.UI;
using Content.Scripts.Scriptable;
using UnityEngine;

namespace Content.Scripts.Game
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private UIPlayerWeapons uiPlayerWeapons;
        public void Init(PlayerInventory inventory, WeaponsConfigObject weaponsConfig)
        {
            uiPlayerWeapons.Init(inventory, weaponsConfig);
        }
    }
}
