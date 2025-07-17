using UnityEngine;

namespace Content.Scripts.Game.Services
{
    public static class InputService
    {
        public static bool IsSpaceDown => Input.GetKeyDown(KeyCode.Space);
        public static bool JumpPressed => Input.GetKey(KeyCode.Space);
        public static bool IsReturnDown => Input.GetKey(KeyCode.Return);
        public static bool IsEscapeDown => Input.GetKey(KeyCode.Escape);
        public static bool CrouchPressed => Input.GetKey(KeyCode.LeftControl);
        public static bool RunPressed => Input.GetKey(KeyCode.LeftShift);
        public static bool IsShootPressed => Input.GetKey(KeyCode.Mouse0);
        public static float MouseX => Input.GetAxis("Mouse X");
        public static bool IsChatPressed => Input.GetKeyDown(KeyCode.T);
    }
}
