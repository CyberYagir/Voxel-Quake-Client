using UnityEngine;

namespace Content.Scripts.Game
{
    public partial class PlayerController
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
    }
}