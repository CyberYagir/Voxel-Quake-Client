using Content.Scripts.Game.UI;
using Content.Scripts.Game.Voxels;
using Content.Scripts.Services;
using Content.Scripts.Services.Net;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game.Services
{
    public class UIService : MonoBehaviour
    {
        [SerializeField] private UIMapSelectionWindow uiMapSelectionWindow;
        [SerializeField] private UILoadingScreen uiLoadingScreen;
        private DataLoaderService dataLoaderService;
        private NetService netService;

        [Inject]
        private void Construct(DataLoaderService dataLoaderService, VoxelVolumeDrawer voxelVolumeDrawer, NetService netService)
        {
            this.netService = netService;
            this.dataLoaderService = dataLoaderService;
            uiLoadingScreen.Init(voxelVolumeDrawer);
            // uiMapSelectionWindow.Init(dataLoaderService, this);

            
        }


        public void ShowLoading()
        {
            uiLoadingScreen.Show();
        }
    }
}
