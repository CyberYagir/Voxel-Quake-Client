using System;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Services.Net
{
    public class NetObject : MonoBehaviour
    {
        [SerializeField] private int peerID;
        [SerializeField] private int localPeerID;


        public bool isMine => peerID == localPeerID;

        public int PeerID => peerID;

        public void Init(int peerID, int localPeerID)
        {
            this.localPeerID = localPeerID;
            this.peerID = peerID;
        }
    }
}
