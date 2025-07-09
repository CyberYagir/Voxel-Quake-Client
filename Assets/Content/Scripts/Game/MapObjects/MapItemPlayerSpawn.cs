using Content.Scripts.Game.Interfaces;
using UnityEngine;

namespace Content.Scripts.Game.MapObjects
{
    public class MapItemPlayerSpawn : MapAdditionalItem, IPlayerSpawn
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.color = Color.green;
            
            Gizmos.DrawLine(Vector3.zero, Vector3.forward);
        }

        public Transform Transform => transform;
    }
}
