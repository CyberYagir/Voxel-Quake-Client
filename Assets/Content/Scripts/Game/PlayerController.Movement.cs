using Content.Scripts.Game.Services;
using UnityEngine;

namespace Content.Scripts.Game
{
    public partial class PlayerController
    {
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
    }
}