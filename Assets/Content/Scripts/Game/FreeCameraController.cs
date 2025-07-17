using Content.Scripts.Game.Interfaces;
using Content.Scripts.Game.Services;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game
{
    public class FreeCameraController : MonoBehaviour, ITeleportable
    {
        [System.Serializable]
        public class FreeMovement
        {
            [SerializeField] private float walkSpeed = 7.0f;
            [SerializeField] private float runSpeed = 7.0f;
            [SerializeField] private Transform directionObject;
            [SerializeField] private CharacterController controller;
            private Vector3 playerVelocity = Vector3.zero;

            private Transform transform;

            public void Init(Transform transform)
            {
                this.transform = transform;
            }

            public void Update()
            {
                Vector3 inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
                inputDir = Vector3.ClampMagnitude(inputDir, 1);

                inputDir = directionObject.TransformDirection(inputDir);
            
                FlyMovement(inputDir);
            
                controller.Move(playerVelocity * Time.deltaTime);
            }

            private void FlyMovement(Vector3 inputDir)
            {
                playerVelocity = inputDir * (InputService.RunPressed ? runSpeed : walkSpeed);
            }

            public void Teleport(Transform teleportableTransform)
            {
                controller.enabled = false;
                transform.position = teleportableTransform.position;
                transform.rotation = teleportableTransform.rotation;
                controller.enabled = true;
            }
        }

        [SerializeField] private PlayerController.HeadBob headBob;
        [SerializeField] private FreeMovement freeMovement;
        private PlayerService playerService;

        public Transform Transform => transform;

        [Inject]
        private void Construct(PlayerService playerService)
        {
            this.playerService = playerService;
            
            headBob.Init(transform);
            freeMovement.Init(transform);
        }

        void Update()
        {
            if (playerService.PlayerState == PlayerService.EPlayerState.Active)
            {
                headBob.Update();
                freeMovement.Update();
            }
        }

        public void SetVelocity(Vector3 velocity)
        {
            //not implement
        }

        public void AddVelocity(Vector3 velocity)
        {
        
        }

        public void Teleport(Transform teleportableTransform)
        {
            freeMovement.Teleport(teleportableTransform);
        }
    }
}
