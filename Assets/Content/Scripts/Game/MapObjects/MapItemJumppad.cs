using System.Collections;
using Content.Scripts.Game.Interfaces;
using UnityEngine;

namespace Content.Scripts.Game.MapObjects
{
    public class MapItemJumppad : MapAdditionalItem
    {
        private bool isDelay = false;
        
        private void OnTriggerStay(Collider other)
        {
            if (isDelay) return;
            
            var player = other.GetComponent<IPlayer>();

            if (player != null)
            {
                var teleportable = player.Transform.GetComponent<ITeleportable>();

                var force = (Vector3)GetKey("force");
                teleportable.AddVelocity(force);
                isDelay = true;
                StartCoroutine(WaitDelay());
                print("Trigger");
            }
        }

        IEnumerator WaitDelay()
        {
            yield return new WaitForSeconds(0.5f);
            isDelay = false;
        }
    }
}
