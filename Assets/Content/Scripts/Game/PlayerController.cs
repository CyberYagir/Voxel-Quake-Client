using System.Collections.Generic;
using Content.Scripts.Game.Interfaces;
using Content.Scripts.Game.Services;
using Content.Scripts.Game.Weapons;
using Content.Scripts.Services.Net;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour, ITeleportable, IPlayer
    {
        [System.Serializable]
        public class HeadBob
        {
            [SerializeField] private float mouseSensitivity = 2.0f;
            [SerializeField] private Transform playerCamera;
            [SerializeField] private Camera camera;
            private float xRotation = 0f;
            private Transform transform;

            public void Init(Transform transform)
            {
                this.transform = transform;
            }

            public void Update()
            {
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);

                playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                transform.Rotate(Vector3.up * mouseX);
            }
            
            public void Update(float cameraX)
            {
                xRotation = cameraX;
                playerCamera.localRotation =
                    Quaternion.Lerp(playerCamera.localRotation, Quaternion.Euler(xRotation, 0f, 0f), Time.deltaTime * 10f);
            }

            public void DisableCamera()
            {
                camera.enabled = false;
                camera.GetComponent<AudioListener>().enabled = false;
            }

            public float CameraX()
            {
                return xRotation;
            }
        }

        [System.Serializable]
        public class Movement
        {
            [SerializeField] private float walkSpeed = 7.0f;
            [SerializeField] private float runAcceleration = 14.0f;
            [SerializeField] private float airAcceleration = 2.0f;
            [SerializeField] private float airControl = 0.3f;
            [SerializeField] private float friction = 6.0f;
            [SerializeField] private float gravity = 20.0f;
            [SerializeField] private float jumpForce = 8.0f;
            [SerializeField] private CharacterController controller;
            [SerializeField] private Vector3 playerVelocity = Vector3.zero;

            private Transform transform;

            public void Init(Transform transform)
            {
                this.transform = transform;
            }

            public void Update()
            {
                Vector3 inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
                inputDir = Vector3.ClampMagnitude(inputDir, 1);
                inputDir = transform.TransformDirection(inputDir);

                if (controller.isGrounded)
                {
                    ApplyFriction();
                    if (InputService.JumpPressed)
                    {
                        playerVelocity.y = jumpForce;
                    }

                    GroundMove(inputDir);
                }
                else
                {
                    AirMove(inputDir);
                }

                CollisionFlags flags = controller.Move(playerVelocity * Time.deltaTime);

                if ((flags & CollisionFlags.Above) != 0 && playerVelocity.y > 0)
                {
                    playerVelocity.y = 0;
                }

                if ((flags & CollisionFlags.Below) != 0 && playerVelocity.y < 0)
                {
                    playerVelocity.y = 0;
                }

                // Стенка — обнуляем только ту часть скорости, которая смотрит в сторону столкновения
                if ((flags & CollisionFlags.Sides) != 0)
                {
                    // Берём направление движения игрока по горизонтали
                    Vector3 moveDir = new Vector3(playerVelocity.x, 0, playerVelocity.z).normalized;

                    // Луч перед игроком
                    Ray ray = new Ray(controller.transform.position, moveDir);
                    if (Physics.Raycast(ray, out RaycastHit hit, controller.radius + 0.1f, LayerMask.GetMask("Default")))
                    {
                        // Получаем нормаль столкновения и обнуляем проекцию скорости на неё
                        Vector3 normal = hit.normal;
                        Vector3 horizontalVel = new Vector3(playerVelocity.x, 0, playerVelocity.z);
                        Vector3 projection = Vector3.Project(horizontalVel, -normal);

                        playerVelocity -= projection;
                    }
                }

            }

            void ApplyFriction()
            {
                Vector3 horizontalVel = new Vector3(playerVelocity.x, 0, playerVelocity.z);
                float speed = horizontalVel.magnitude;

                if (speed != 0)
                {
                    float drop = speed * friction * Time.deltaTime;
                    float newSpeed = Mathf.Max(speed - drop, 0);
                    playerVelocity.x *= newSpeed / speed;
                    playerVelocity.z *= newSpeed / speed;
                }
            }

            void GroundMove(Vector3 wishDir)
            {
                float wishSpeed = walkSpeed;
                Accelerate(wishDir, wishSpeed, runAcceleration);
            }

            void AirMove(Vector3 wishDir)
            {
                float wishSpeed = walkSpeed;
                Accelerate(wishDir, wishSpeed, airAcceleration);

                // Quake-style air control
                if (Vector3.Dot(playerVelocity, wishDir) > 0)
                {
                    float speed = playerVelocity.magnitude;
                    Vector3 velocityNorm = playerVelocity.normalized;

                    float dot = Vector3.Dot(velocityNorm, wishDir);
                    float k = 32f; // чем выше, тем больше влияние air control
                    float control = k * airControl * dot * dot * Time.deltaTime;

                    playerVelocity += wishDir * control;
                }

                // Применение гравитации
                playerVelocity.y -= gravity * Time.deltaTime;
            }

            void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
            {
                float currentSpeed = Vector3.Dot(playerVelocity, wishDir);
                float addSpeed = wishSpeed - currentSpeed;

                if (addSpeed <= 0)
                    return;

                float accelSpeed = accel * Time.deltaTime * wishSpeed;
                if (accelSpeed > addSpeed)
                    accelSpeed = addSpeed;

                playerVelocity += wishDir * accelSpeed;
            }

            public void SetVelocity(Vector3 velocity)
            {
                playerVelocity = velocity;
            }
            public void AddVelocity(Vector3 velocity)
            {
                playerVelocity.x += velocity.x;
                playerVelocity.z += velocity.z;
                if (velocity.y > 0)
                {
                    playerVelocity.y = velocity.y;
                }
                else
                {
                    playerVelocity.y += velocity.y;
                }
            }
            public void Teleport(Transform teleportableTransform)
            {
                controller.enabled = false;
                transform.position = teleportableTransform.position;
                transform.rotation = teleportableTransform.rotation;
                controller.enabled = true;
            }


            public Vector3 GetLocalVel()
            {
                return  transform.InverseTransformDirection(playerVelocity);
            }

            public Vector3 GetVelocity()
            {
                return controller.velocity;
            }
        }
    
        [System.Serializable]
        public class WeaponMove
        {
            [SerializeField] private float speed;
            [SerializeField] private float moveModify;
            [SerializeField] private Transform holder;
    
            private Vector3 pos;
            private Vector3 oldPlayerPos;
            private Quaternion rotation;
            private Transform transform;


            public void Init(Transform transform)
            {
                this.transform = transform;
                pos = holder.localPosition;
                rotation = holder.rotation;
                oldPlayerPos = transform.position;
            }


            public void Update()
            {
                var deltaPos = oldPlayerPos - transform.position;

                oldPlayerPos = transform.position;

                holder.localPosition = Vector3.Lerp(holder.localPosition,
                    pos + holder.parent.InverseTransformDirection(deltaPos) * moveModify, Time.deltaTime * 5);
            
            
                holder.localEulerAngles = Vector3.zero;

                var targetPos = holder.rotation;

                rotation = Quaternion.Lerp(rotation, targetPos, Time.deltaTime * speed);
                holder.rotation = rotation;
            }

            public void SetDeltaPos()
            {
                oldPlayerPos = transform.position;
            }

        }
    
    
        [System.Serializable]
        public class WeaponManager
        {
            [SerializeField] private List<WeaponBase> weaponBases;
            [SerializeField] private Camera camera;
            [SerializeField] private Camera weaponCamera;
            [SerializeField] private Transform cameraLocalPosSwitch;
            [SerializeField] private AnimationCurve switchCurve;
            [SerializeField, ReadOnly] private bool isWeaponSwitch;
            private int selectedWeapon = 0;
            private PrefabSpawnerFabric prefabSpawnerFabric;
            private NetServiceProjectiles netServiceProjectiles;

            public void Init(PrefabSpawnerFabric prefabSpawnerFabric, NetServiceProjectiles netServiceProjectiles)
            {
                this.netServiceProjectiles = netServiceProjectiles;
                this.prefabSpawnerFabric = prefabSpawnerFabric;
                ActiveWeapon(selectedWeapon);
            }

            private void ActiveWeapon(int n)
            {
                selectedWeapon = n;

                isWeaponSwitch = true;

                var duration = 0.5f;
                var isSwitched = false;
                var startPos = weaponCamera.transform.localPosition;
                var startRot = weaponCamera.transform.localRotation;
                
                DOVirtual.Float(0, 1f, duration, delegate(float v)
                {
                    weaponCamera.transform.localPosition = Vector3.Lerp(startPos, cameraLocalPosSwitch.localPosition, v);
                    weaponCamera.transform.localRotation = Quaternion.Lerp(startRot, cameraLocalPosSwitch.localRotation, v);

                    if (v >= 0.5f && !isSwitched)
                    {
                        for (int i = 0; i < weaponBases.Count; i++)
                        {
                            weaponBases[i].gameObject.SetActive(i == n);
                            if (i == n)
                            {
                                weaponBases[i].Init(prefabSpawnerFabric, camera, netServiceProjectiles);
                            }
                        }

                        isSwitched = true;
                    }
                    
                    print(v);
                }).SetEase(switchCurve).onComplete += delegate
                {
                    isWeaponSwitch = false;
                };
                
                print(isWeaponSwitch);
                
            }

            public void Update()
            {
                if (isWeaponSwitch) return;
                
                if (InputService.IsShootPressed)
                {
                    for (int i = 0; i < weaponBases.Count; i++)
                    {
                        if (weaponBases[i].gameObject.activeInHierarchy)
                        {
                            weaponBases[i].Shoot();
                            break;
                        }
                    }
                }

                for (int i = 1; i < 9; i++)
                {
                    if (Input.GetKeyDown(i.ToString()))
                    {
                        var id = i - 1;
                        if (id < weaponBases.Count)
                        {
                            if (selectedWeapon != id)
                            {
                                if (weaponBases[selectedWeapon].isCanShoot)
                                {
                                    ActiveWeapon(id);
                                }
                            }
                        }
                    }
                }
            
                weaponBases[selectedWeapon].UpdateWeapon();
            }
            
            public void DisableCamera()
            {
                camera.enabled = false;
                foreach (var weaponBase in weaponBases)
                {
                    weaponBase.gameObject.ChangeLayerWithChilds(LayerMask.NameToLayer("Default"));
                }
            }
        }

        [SerializeField] private NetObject netObject;
        [SerializeField] private CapsuleCollider capsuleCollider;
        [SerializeField] private Canvas playerUI;
        [SerializeField] private NetPlayerController netPlayerController;
        
        [SerializeField] private HeadBob headBob;
        [SerializeField] private Movement movement;
        [SerializeField] private WeaponMove weaponMove;
        [SerializeField] private WeaponManager weaponManager;
        private NetServiceProjectiles netProjectiles;
        public Transform Transform => transform;

        public NetPlayerController NetController => netPlayerController;

        [Inject]
        private void Construct(PrefabSpawnerFabric spawnerFabric, NetService netService)
        {
            netProjectiles = netService.GetModule<NetServiceProjectiles>();
            
            if (netObject.isMine)
            {
                headBob.Init(transform);
                movement.Init(transform);
                weaponMove.Init(transform);
                weaponManager.Init(spawnerFabric, netProjectiles);
            }
            else
            {
                headBob.DisableCamera();
                weaponManager.DisableCamera();
                playerUI.gameObject.SetActive(false);
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