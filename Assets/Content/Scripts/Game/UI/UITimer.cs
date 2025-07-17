using System;
using Content.Scripts.Services.Net;
using TMPro;
using UnityEngine;

namespace Content.Scripts.Game.UI
{
    public class UITimer : MonoBehaviour
    {
        private NetServiceServer netServiceServer;
        [SerializeField] private TMP_Text text;

        public void Init(NetServiceServer netServiceServer)
        {
            this.netServiceServer = netServiceServer;
            text.text = "Warmup";
            netServiceServer.Server.OnTimerTick += ServerOnOnTimerTick;
        }

        private void ServerOnOnTimerTick(float obj)
        {
            var span = new TimeSpan(0, 0, (int)obj);
            text.text = span.Minutes.ToString("00") + ":" + span.Seconds.ToString("00");
        }

        private void OnDestroy()
        {
            netServiceServer.Server.OnTimerTick -= ServerOnOnTimerTick;
        }
    }
}
