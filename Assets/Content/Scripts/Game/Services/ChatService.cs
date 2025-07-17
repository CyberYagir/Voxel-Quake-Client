using System;
using Content.Scripts.Services.Net;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game.Services
{
    public class ChatService : MonoBehaviour
    {
        private PlayerService playerService;
        private NetServiceChat chatModule;
        public event Action<bool> OnChatActiveChanged;
        public event Action OnSendMessage;
        public event Action<string, string> OnMessageRecieved;

        [Inject]
        private void Construct(PlayerService playerService, NetService netService)
        {
            this.playerService = playerService;

            chatModule = netService.GetModule<NetServiceChat>();
            chatModule.OnMessageReceived += (playerName, message) =>
            {
                OnMessageRecieved?.Invoke(playerName, message);
            };
        }
        
        

        private void Update()
        {
            if (InputService.IsChatPressed)
            {
                if (playerService.PlayerState == PlayerService.EPlayerState.Active)
                {
                    playerService.ChangeState(PlayerService.EPlayerState.Chat);
                    OnChatActiveChanged?.Invoke(true);
                }
            }

            if (playerService.PlayerState == PlayerService.EPlayerState.Chat)
            {
                if (InputService.IsReturnDown)
                {
                    OnSendMessage?.Invoke();
                    OnChatActiveChanged?.Invoke(false);
                    playerService.ChangeState(PlayerService.EPlayerState.Active);
                }
                
                if (InputService.IsEscapeDown)
                {
                    OnChatActiveChanged?.Invoke(false);
                    playerService.ChangeState(PlayerService.EPlayerState.Active);
                }
            }
        }

        public void SendMessageRPC(string inputFieldText)
        {
            chatModule.SendMessageRPC(inputFieldText);
        }
    }
}
