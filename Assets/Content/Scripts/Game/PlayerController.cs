using System;
using Content.Scripts.Game.Interfaces;
using Content.Scripts.Game.Services;
using Content.Scripts.Scriptable;
using Content.Scripts.Services.Net;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using Zenject.SpaceFighter;

namespace Content.Scripts.Game
{
    [RequireComponent(typeof(CharacterController))]
    public partial class PlayerController : MonoBehaviour, ITeleportable, IPlayer
    {
        [SerializeField] private NetObject netObject;
        [SerializeField] private NetPlayerController netPlayerController;
        [SerializeField] private PlayerAnimator playerAnimator;
        [Space] [SerializeField] private PlayerInventory inventory;
        [SerializeField] private PlayerUI userInterface;
        [Space] [SerializeField] private CapsuleCollider capsuleCollider;
        [Space] [SerializeField] private HeadBob headBob;
        [SerializeField] private Movement movement;
        [SerializeField] private WeaponMove weaponMove;
        [SerializeField] private WeaponManager weaponManager;

        private NetServiceProjectiles netProjectiles;
        private PlayerService playerService;
        public Transform Transform => transform;

        public NetPlayerController NetController => netPlayerController;
        public bool IsMine => netObject.isMine;

        public PlayerAnimator PlayerAnimator => playerAnimator;

        [Inject]
        private void Construct(PrefabSpawnerFabric spawnerFabric, NetService netService,
            WeaponsConfigObject weaponsConfig, PlayerService playerService)
        {
            this.playerService = playerService;
            netProjectiles = netService.GetModule<NetServiceProjectiles>();

            if (netObject.isMine)
            {
                headBob.Init(transform);
                movement.Init(transform);
                weaponMove.Init(transform);
                inventory.Init();
                userInterface.Init(inventory, weaponsConfig);
                weaponManager.Init(spawnerFabric, netProjectiles, weaponsConfig, inventory);

                weaponManager.OnSelectedWeaponChange += o =>
                {
                    netService.GetModule<NetServicePlayers>().RPCChangeWeapon(o.Type);
                };
            }
            else
            {
                headBob.DisableCamera();
                weaponManager.DisableCamera();
                userInterface.Disable();
            }

            playerService.OnPlayerStateChange += (state) =>
            {
                if (state != PlayerService.EPlayerState.Active)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            };
        }

        public Vector3 ClosestPoint(Vector3 point)
        {
            return capsuleCollider.ClosestPoint(point);
        }

        void Update()
        {
            if (netObject.isMine)
            {
                if (playerService.PlayerState == PlayerService.EPlayerState.Active)
                {
                    headBob.Update();
                    movement.Update();
                    weaponManager.Update();
                }

                movement.PhysicsUpdate();
            }
        }

        private void LateUpdate()
        {
            if (netObject.isMine)
            {
                weaponMove.Update();
            }
        }

        public void SetVelocity(Vector3 velocity)
        {
            movement.SetVelocity(velocity);
        }

        public void AddVelocity(Vector3 velocity)
        {
            movement.AddVelocity(velocity);
        }

        public void Teleport(Transform teleportableTransform)
        {
            movement.Teleport(teleportableTransform);
            weaponMove.SetDeltaPos();
        }

        public Vector3 GetVelocity()
        {
            return movement.GetVelocity();
        }

        public float CameraX()
        {
            return headBob.CameraX();
        }

        public void SetNextCameraX(float nextCameraX)
        {
            headBob.Update(nextCameraX);
        }
    }
}