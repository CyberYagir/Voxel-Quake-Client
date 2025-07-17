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
        // [SerializeField] private UIMapSelectionWindow uiMapSelectionWindow;
        [SerializeField] private UITimer timer;
        [SerializeField] private UILoadingScreen uiLoadingScreen;
        [SerializeField] private UIChat uiChat;
        private DataLoaderService dataLoaderService;
        private NetService netService;

        [Inject]
        private void Construct(DataLoaderService dataLoaderService, VoxelVolumeDrawer voxelVolumeDrawer, NetService netService, ChatService chatService)
        {
            this.netService = netService;
            this.dataLoaderService = dataLoaderService;
            uiLoadingScreen.Init(voxelVolumeDrawer);
            timer.Init(netService.GetModule<NetServiceServer>());
            uiChat.Init(chatService);
        }


        public void ShowLoading()
        {
            uiLoadingScreen.Show();
        }
    }
}
