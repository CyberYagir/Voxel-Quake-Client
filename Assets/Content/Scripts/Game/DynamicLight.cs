using System;
using System.Collections.Generic;
using Content.Scripts.Game.Services;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zenject;

namespace Content.Scripts.Game
{
    public class DynamicLight : MonoBehaviour
    {
        [System.Serializable]
        public class LightBounds
        {
            public Vector3 offset;
            public Vector3 size;
        }
        
        [System.Serializable]
        public class LightBoundsList
        {
            public List<LightBounds> boundsList = new List<LightBounds>();
        }
        
        [SerializeField] private List<Light> lights = new List<Light>();
        private bool isHasBounds;
        private PlayerService playerService;
        private LightBoundsList lightBounds;


        private List<Bounds> boundsList = new List<Bounds>();

        public bool IsHasBounds => isHasBounds;

        private bool isPlayerInside;

        [Inject]
        private void Construct(PlayerService playerService)
        {
            this.playerService = playerService;
        }

        public void Init(LightBoundsList lightBounds)
        {
            this.lightBounds = lightBounds;
            if (lightBounds != null)
            {
                isHasBounds = true;

                for (int i = 0; i < lightBounds.boundsList.Count; i++)
                {

                    boundsList.Add(new Bounds(transform.position + lightBounds.boundsList[i].offset, lightBounds.boundsList[i].size));
                }
            }
        }

        public void UpdateLight()
        {
            if (!isHasBounds) return;

            isPlayerInside = false;
            for (int i = 0; i < boundsList.Count; i++)
            {
                if (boundsList[i].Contains(playerService.GetPlayerPosition()))
                {
                    isPlayerInside = true;
                    break;
                }
            }
            
            
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].enabled = isPlayerInside;
            }
        }

        private void OnDrawGizmos()
        {
            if (isHasBounds && isPlayerInside)
            {
                for (int i = 0; i < lightBounds.boundsList.Count; i++)
                {
                    Gizmos.DrawWireCube(transform.position + lightBounds.boundsList[i].offset, lightBounds.boundsList[i].size);
                }
            }
        }

        public void SetShadows(bool state)
        {
            if (!state)
            {
                for (int i = 0; i < lights.Count; i++)
                {
                    lights[i].shadows = LightShadows.None;
                }
            }
        }
    }
}
