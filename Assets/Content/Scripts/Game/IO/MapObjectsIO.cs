using System.IO;
using Content.Scripts.Game.Scriptable;
using Newtonsoft.Json;
using UnityEngine;

namespace Content.Scripts.Game.IO
{
    public class MapObjectsIO : IFileInput<AdditionalObjectsData>, IFileOutput
    {
        
        private AdditionalObjectsData targetData;

        public JsonSerializerSettings GetSerializerSettings() => new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new IgnorePropertiesResolver(new[] { "magnitude", "sqrMagnitude", "normalized" }),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };


        private Transform holder;
        private MapAdditionalListObject itemsList;
        public void PrepareSave(Transform holder, MapAdditionalListObject mapAdditionalListObject)
        {
            this.holder = holder;
            itemsList = mapAdditionalListObject;
        }

        public void SaveData(string path)
        {
            var allObjects = holder.GetComponentsInChildren<MapAdditionalItem>();
            targetData = new AdditionalObjectsData();

            for (int i = 0; i < allObjects.Length; i++)
            {
                targetData.AddItem(new AdditionalObjectsData.Item(
                    itemsList.GetIDByPrefab(allObjects[i]),
                    allObjects[i].transform.position,
                    allObjects[i].transform.eulerAngles,
                    allObjects[i].Data
                ));
            }

            Debug.Log(path);
            File.WriteAllText(path, JsonConvert.SerializeObject(targetData, Formatting.Indented, GetSerializerSettings()));
        }
        
        
        public AdditionalObjectsData LoadData(string path)
        {
            if (File.Exists(path))
            {
                targetData = JsonConvert.DeserializeObject<AdditionalObjectsData>(File.ReadAllText(path), GetSerializerSettings());
                return targetData;
            }

            return null;
        }
    }
}