using System.Collections.Generic;
using UnityEngine;

namespace Content.Scripts.Game.Scriptable
{
    [CreateAssetMenu(menuName = "Create MapAddtionalListObject", fileName = "MapAddtionalListObject", order = 0)]
    public class MapAdditionalListObject : ScriptableObject
    {
        
        [SerializeField] private List<MapAdditionalItem> items;



        public MapAdditionalItem GetItemByID(string id)
        {
            var item = items.Find(x => x.ID.ToLower() == id.ToLower().Trim());

            if (item == null)
            {
                return null;
            }
            return item;
        }


        public string GetIDByPrefab(MapAdditionalItem prefab)
        {
            var item = items.Find(x => x.ID == prefab.ID);

            if (item == null)
            {
                return null;
            }

            return item.ID;
        }
    }
}
