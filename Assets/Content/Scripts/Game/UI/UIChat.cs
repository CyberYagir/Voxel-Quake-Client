using System;
using System.Collections.Generic;
using Content.Scripts.Game.Services;
using Content.Scripts.Services.Net;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Content.Scripts.Game.UI
{
    public class UIChat : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text chatText;
        [SerializeField] private CanvasGroup canvasGroup;
        private List<string> messages = new List<string>();
        private ChatService chatService;

        private float hideTimer;
        private bool tween;
        private bool state;

        public void Init(ChatService chatService)
        {
            this.chatService = chatService;
            chatService.OnChatActiveChanged += ChatActive;
            chatService.OnSendMessage += OnSendMessage;
            chatService.OnMessageRecieved += OnMessageRecieved;
            ChatActive(false);
            chatText.text = string.Empty;
        }

        private void Update()
        {
            if (!state && !tween)
            {
                hideTimer += Time.deltaTime;
                if (hideTimer >= 5)
                {
                    canvasGroup.DOFade(0, 0.25f);
                    tween = true;
                    hideTimer = 0;
                }
            }
        }

        private void OnMessageRecieved(string playerName, string message)
        {
            messages.Add($"[{playerName}] " + message);

            if (messages.Count > 6)
            {
                messages.RemoveAt(0);
            }

            chatText.text = string.Empty;
            for (int i = 0; i < messages.Count; i++)
            {
                chatText.text += messages[i] + "\n";
            }


            ResetChatState();
        }

        private void OnSendMessage()
        {
            var str = inputField.text.Trim();
            if (str.Length > inputField.characterLimit)
            {
                str = str.Substring(0, inputField.characterLimit);
            }

            inputField.DeactivateInputField();
            if (str.Length > 0)
            {
                chatService.SendMessageRPC(inputField.text);
            }

            inputField.text = string.Empty;
        }

        private void ChatActive(bool obj)
        {
            state = obj;
            inputField.gameObject.SetActive(obj);
            if (obj)
            {
                inputField.ActivateInputField();
                ResetChatState();
            }
        }

        private void ResetChatState()
        {
            canvasGroup.DOKill();
            canvasGroup.alpha = (1f);
            tween = false;
            hideTimer = 0;
        }
    }
}
