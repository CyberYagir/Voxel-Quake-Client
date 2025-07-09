using System.Collections.Generic;
using Content.Scripts.Game.Services;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Content.Scripts.Game
{
    public class MapAdditionalItem : SerializedMonoBehaviour
    {
        [SerializeField] private string id;
        [SerializeField] private Dictionary<string, object> data = new Dictionary<string, object>();
        private MapObjectsService mapObjectsService;
        public string ID => id;

        public Dictionary<string, object> Data => data;


        public void SetData(Dictionary<string, object> loadedData)
        {
            data = loadedData;
        }

        public virtual void Init(MapObjectsService mapObjectsService)
        {
            this.mapObjectsService = mapObjectsService;
        }

        public object GetKey(string key)
        {
            if (data.ContainsKey(key))
            {
                return data[key];
            }

            return null;
        }
    }
}