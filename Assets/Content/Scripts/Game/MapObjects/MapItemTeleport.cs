using Content.Scripts.Game.Interfaces;
using Content.Scripts.Game.Services;
using Content.Scripts.Services.Net;
using UnityEngine;

namespace Content.Scripts.Game.MapObjects
{
    public class MapItemTeleport : MapAdditionalItem
    {
        [SerializeField] MapItemTeleportExit teleportLocation;
        private void OnDrawGizmos()
        {
            
            Gizmos.color = Color.magenta;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            
            if (teleportLocation == null) return;

        }

        public override void Init(MapObjectsService mapObjectsService, int uid, NetService netService)
        {
            base.Init(mapObjectsService, uid, netService);

            var teleport_id = GetKey("teleport_id");
            foreach (var mapAdditionalItem in mapObjectsService.SpawnedItems)
            {
                if (mapAdditionalItem is MapItemTeleportExit)
                {
                    var id = mapAdditionalItem.GetKey("teleport_id");

                    if (teleport_id == id)
                    {
                        teleportLocation = mapAdditionalItem as MapItemTeleportExit;
                        break;
                    }
                }
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (teleportLocation == null) return;
            var teleportable = other.GetComponent<ITeleportable>();

            if (teleportable != null)
            {
                teleportable.Teleport(teleportLocation.transform);
                teleportable.SetVelocity(teleportLocation.transform.forward * 10);
            }
        }
    }
}
