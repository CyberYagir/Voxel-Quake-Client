using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Content.Scripts.Game.IO
{
    [System.Serializable]
    public class AdditionalObjectsData
    {
        [System.Serializable]
        public class Item
        {
            [System.Serializable]
            public class KeyPair
            {
                [JsonProperty("Key")]
                public string Key { get; set; }
                
                [JsonProperty("Value")]
                public object Value { get; set; }

                public KeyPair(string key, object value)
                {
                    this.Key = key;
                    this.Value = value;
                }

                public KeyPair()
                {
                }
            }
            
            [JsonProperty("ID")]
            public string ID { get; set; }

            [JsonProperty("Position")]
            public Vector3 Position { get; set; }

            [JsonProperty("Rotation")]
            public Vector3 Rotation { get; set; }


            [JsonProperty("Data")] 
            public List<KeyPair> Data { get; set; } = new List<KeyPair>();

            public Item(string id, Vector3 position, Vector3 rotation, Dictionary<string, object> objectData)
            {
                this.ID = id;
                this.Position = position;
                this.Rotation = rotation;


                foreach (var d in objectData)
                {
                    Data.Add(new KeyPair(d.Key, d.Value));
                }
            }

            public Item()
            {
            }

            public Dictionary<string, object> GetDataDictionary()
            {
                var data = new Dictionary<string, object>();

                for (int i = 0; i < Data.Count; i++)
                {
                    data.Add(Data[i].Key, Data[i].Value);
                }

                return data;
            }
        }
        
        [JsonProperty("Items")]
        public List<Item> Items { get; set; } = new List<Item>();

        public void AddItem(Item item)
        {
            Items.Add(item);
        }
    }
}