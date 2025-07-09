using UnityEngine;
using Zenject;

namespace Content.Scripts.Game.Services
{
    public class PrefabSpawnerFabric : IFactory
    {
        private DiContainer container;

        [Inject]
        public PrefabSpawnerFabric(DiContainer container)
        {
            this.container = container;
        }


        public T SpawnItem<T>(T prefab, Vector3 pos = default, Quaternion rot = default, Transform parent = null) where T : Object
        {
            return container.InstantiatePrefabForComponent<T>(prefab, pos, rot, parent);
        }

        public GameObject SpawnItem(GameObject prefab, Vector3 pos = default, Quaternion rot = default, Transform parent = null)
        {
            return container.InstantiatePrefab(prefab, pos, rot, parent);
        }

        public T SpawnItem<T>(T prefab, Transform parent) where T : Object
        {
            return container.InstantiatePrefabForComponent<T>(prefab, parent);
        }


        public GameObject SpawnItem(GameObject prefab, Transform parent)
        {
            return container.InstantiatePrefab(prefab, parent);
        }
        
        
        public T SpawnItemOnGround<T>(T prefab, Vector3 pos = default, Quaternion rot = default, Transform parent = null, int layer = ~0, float yOffcet = 1) where T : Object
        {
            Vector3 spawnPos = pos;
            if (Physics.Raycast(pos + Vector3.up * 100, Vector3.down, out RaycastHit hit, Mathf.Infinity, layer, QueryTriggerInteraction.Ignore))
            {
                spawnPos = hit.point + Vector3.up * yOffcet;
            }

            var item = SpawnItem<T>(prefab, spawnPos, rot, parent);
            
            return item;
        }

        public void InjectComponent(Object getComponent)
        {
            if (getComponent != null)
            {
                if (getComponent is GameObject g)
                {
                    container.InjectGameObject(g);
                }
                else
                {
                    container.Inject(getComponent);
                }
            }
        }
    }
}
