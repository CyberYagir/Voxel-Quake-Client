using UnityEngine;

namespace Content.Scripts.Game.Interfaces
{
    public interface ITeleportable
    {
        Transform Transform { get; }
    
        public void SetVelocity(Vector3 velocity);
        public void AddVelocity(Vector3 velocity);
        void Teleport(Transform teleportableTransform);
    }
}