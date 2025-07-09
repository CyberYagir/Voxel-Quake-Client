using UnityEngine;

namespace Content.Scripts.Game.MapObjects
{
    public class MapItemTeleportExit : MapAdditionalItem
    {
        private void OnDrawGizmos()
        {
                        
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Vector3.zero, Vector3.forward);
        }
    }
}
