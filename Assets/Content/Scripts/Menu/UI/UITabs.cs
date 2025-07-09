using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Content.Scripts.Menu.UI
{
    public class UITabs : MonoBehaviour
    {
        [System.Serializable]
        public class Tab
        {
            [SerializeField] private RectTransform rect;
            [SerializeField] private Image background;
            [SerializeField] private LayoutElement layout;
            [SerializeField] private GameObject tab;

            public void Active(bool b)
            {
                tab.gameObject.SetActive(b);

                if (b)
                {
                    background.DOColor(new Color(1, 0.512f, 0, 1), 0.25f);
                }
                else
                {
                    background.DOColor(new Color(1, 0.512f, 0, 0), 0.25f);
                }

                var start = layout.minWidth;
                DOVirtual.Float(0, 1f, 0.25f, delegate(float v)
                {
                    if (b)
                    {
                        layout.minWidth = Mathf.Lerp(start, 150, v);
                    }
                    else
                    {
                        layout.minWidth = Mathf.Lerp(start, 0, v);
                    }
                });

                tab.gameObject.SetActive(b);
            }
        }

        [SerializeField] private List<Tab> tabs = new List<Tab>();
        [SerializeField] private int currentTab = -1;

        public void Init()
        {
            ChangeTab(0);
        }
    
        public void ChangeTab(int newTab)
        {
            if (newTab != currentTab)
            {
                for (int i = 0; i < tabs.Count; i++)
                {
                    tabs[i].Active(i == newTab);
                }
                
                currentTab = newTab;
            }
        }
    }
}
