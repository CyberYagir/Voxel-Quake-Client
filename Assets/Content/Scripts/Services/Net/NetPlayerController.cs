using System;
using Content.Scripts.Game;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Services.Net
{
    public class NetPlayerController : MonoBehaviour
    {
        [SerializeField] private NetObject netObject;
        
        private Vector3 lastPosition;
        private Vector3 lastRot;
        private float oldCameraX;
        
        private NetService netService;
        private NetServicePlayers playersModule;
        
        
        private Vector3 nextPosition;
        private Vector3 nextRot;
        private Vector3 nextVel;
        private PlayerController controller;
        private float nextCameraX;

        [Inject]
        private void Construct(NetService netService)
        {
            this.netService = netService;
            controller = GetComponent<PlayerController>();
            playersModule = netService.GetModule<NetServicePlayers>();
        }

        private void FixedUpdate()
        {
            if (netObject.isMine)
            {
                Quaternion currentRot = transform.rotation;
                Quaternion lastQuat = Quaternion.Euler(lastRot);

                float angleDelta = Quaternion.Angle(currentRot, lastQuat);
                
                if ((lastPosition - transform.position).magnitude > 0.1f || 
                    angleDelta > 1f || 
                    Mathf.Abs(oldCameraX - controller.CameraX()) > 1f)
                {
                    playersModule.RPCUpdatePlayerTransform(transform.position, transform.eulerAngles, controller.GetVelocity(), controller.CameraX());
                    lastRot = transform.eulerAngles;
                    lastPosition = transform.position;
                    oldCameraX = controller.CameraX();
                }
            }
        }

        private void Update()
        {
            if (!netObject.isMine)
            {
                nextVel.y = 0;
                Vector3 predictedPosition = nextPosition + nextVel * 0.05f;
                transform.position = Vector3.Lerp(transform.position, predictedPosition, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(nextRot), Time.deltaTime * 10f);
                controller.SetNextCameraX(nextCameraX);
            }
        }

        public void SetNextTransform(Vector3 pos, Vector3 rot, Vector3 vel, float cameraX)
        {
            nextCameraX = cameraX;
            nextPosition = pos;
            nextRot = rot;
            nextVel = vel;
        }
    }
}
