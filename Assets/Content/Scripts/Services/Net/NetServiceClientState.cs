using System.Collections;
using LightServer.Base.PlayersModule;
using ServerLibrary.Structs;
using UnityEngine;

namespace Content.Scripts.Services.Net
{
    [System.Serializable]
    public class NetServiceClientState : NetServiceModule
    {
        [SerializeField] private EClientState clientState = EClientState.Connecting;
        private ScenesService scenesService;


        public NetServiceClientState(NetService netService, ScenesService scenesService) : base(netService)
        {
            this.scenesService = scenesService;
            netService.StartCoroutine(WaitForScene());
        }

        IEnumerator WaitForScene()
        {
            while (scenesService.GetActiveScene() != ScenesService.EScene.Game)
            {
                yield return null;
            }

            yield return null;
            clientState = EClientState.InGame;
            netService.Peer.RPCChangeState(clientState);
        }
    }
}