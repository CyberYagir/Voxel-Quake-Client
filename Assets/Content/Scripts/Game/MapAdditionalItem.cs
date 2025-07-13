using System.Collections.Generic;
using Content.Scripts.Game.Services;
using Content.Scripts.Services.Net;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Content.Scripts.Game
{
    public class MapAdditionalItem : SerializedMonoBehaviour
    {
        [SerializeField] private int uid;
        [SerializeField] private string id;
        [SerializeField] private Dictionary<string, object> data = new Dictionary<string, object>();
        private MapObjectsService mapObjectsService;
        protected NetService netService;
        public string ID => id;

        public Dictionary<string, object> Data => data;

        public int Uid => uid;


        public virtual void SetData(Dictionary<string, object> loadedData)
        {
            data = loadedData;
        }

        public virtual void Init(MapObjectsService mapObjectsService, int uid, NetService netService)
        {
            this.netService = netService;
            this.mapObjectsService = mapObjectsService;
            this.uid = uid;
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