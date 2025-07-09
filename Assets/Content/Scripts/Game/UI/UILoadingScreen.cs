using System.Collections;
using Content.Scripts.Game.Voxels;
using TMPro;
using UnityEngine;

namespace Content.Scripts.Game.UI
{
    public class UILoadingScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text percentageText;
        private VoxelVolumeDrawer voxelVolumeDrawer;

        public void Init(VoxelVolumeDrawer voxelVolumeDrawer)
        {
            percentageText.text = "0%";
            this.voxelVolumeDrawer = voxelVolumeDrawer;
            gameObject.SetActive(true);


            voxelVolumeDrawer.OnMapGenerationStart += () =>
            {
                gameObject.SetActive(true);
            };

            voxelVolumeDrawer.OnMapGenerated += () =>
            {
                gameObject.SetActive(false);
            };
        }

        private void OnEnable()
        {
            StartCoroutine(DrawPercentage());
        }


        IEnumerator DrawPercentage()
        {
            percentageText.text = "0%";
            while (true)
            {
                while (voxelVolumeDrawer.StartChunksToLoad == 0 && voxelVolumeDrawer.ChunksToDraw == 0)
                {
                    yield return null;
                }

                yield return null;
                percentageText.text =
                    (((voxelVolumeDrawer.StartChunksToLoad - voxelVolumeDrawer.ChunksToDraw) /
                      (float)voxelVolumeDrawer.StartChunksToLoad) * 100f).ToString("F0") + "%";
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}
