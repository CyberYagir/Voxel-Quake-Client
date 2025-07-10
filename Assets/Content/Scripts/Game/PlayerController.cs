using Content.Scripts.Game.Interfaces;
using Content.Scripts.Game.Services;
using Content.Scripts.Scriptable;
using Content.Scripts.Services.Net;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Content.Scripts.Game
{
    [RequireComponent(typeof(CharacterController))]
    public partial class PlayerController : MonoBehaviour, ITeleportable, IPlayer
    {
        [SerializeField] private NetObject netObject;
        [SerializeField] private NetPlayerController netPlayerController;
        [Space]
        [SerializeField] private PlayerInventory inventory;
         [SerializeField] private PlayerUI userInterface;
        [Space]
        [SerializeField] private CapsuleCollider capsuleCollider;
        [Space]
        [SerializeField] private HeadBob headBob;
        [SerializeField] private Movement movement;
        [SerializeField] private WeaponMove weaponMove;
        [SerializeField] private WeaponManager weaponManager;
        
        private NetServiceProjectiles netProjectiles;
        public Transform Transform => transform;

        public NetPlayerController NetController => netPlayerController;

        [Inject]
        private void Construct(PrefabSpawnerFabric spawnerFabric, NetService netService, WeaponsConfigObject weaponsConfig)
        {
            netProjectiles = netService.GetModule<NetServiceProjectiles>();
            
            if (netObject.isMine)
            {
                headBob.Init(transform);
                movement.Init(transform);
                weaponMove.Init(transform);
                inventory.Init();
                userInterface.Init(inventory, weaponsConfig);
                weaponManager.Init(spawnerFabric, netProjectiles, weaponsConfig, inventory);
            }
            else
            {
                headBob.DisableCamera();
                weaponManager.DisableCamera();
                userInterface.gameObject.SetActive(false);
            }
        }

        public Vector3 ClosestPoint(Vector3 point)
        {
            return capsuleCollider.ClosestPoint(point);
        }

        void Update()
        {
            if (netObject.isMine)
            {
                if (InputService.IsShootPressed)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }

                headBob.Update();
                movement.Update();
                weaponMove.Update();
                weaponManager.Update();
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