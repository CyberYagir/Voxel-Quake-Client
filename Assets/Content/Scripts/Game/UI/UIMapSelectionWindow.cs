using System.Collections;
using Content.Scripts.Game.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Content.Scripts.Game.UI
{
    public class UIMapSelectionWindow : MonoBehaviour
    {
        [SerializeField] private Button item;
        private DataLoaderService dataLoaderService;
        private UIService uiService;

        public void Init(DataLoaderService dataLoaderService, UIService uiService)
        {
            this.uiService = uiService;
            this.dataLoaderService = dataLoaderService;

            foreach (var map in dataLoaderService.Maps)
            {
                var spawnedItem = Instantiate(item, item.transform.parent);
                spawnedItem.gameObject.SetActive(true);
                var mapPath = map;
                
                spawnedItem.onClick.AddListener(delegate
                {
                    LoadMap(map);
                });

                spawnedItem.GetComponentInChildren<TMP_Text>().text = map.MapName;
            }
            
            gameObject.SetActive(true);
        }

        private void LoadMap(DataLoaderService.MapPaths map)
        {
            uiService.ShowLoading();
            StartCoroutine(SkipFrame());

            IEnumerator SkipFrame()
            {
                yield return null;
                
                dataLoaderService.LoadMap(map);
                gameObject.SetActive(false);
            }
        }
    }
}
