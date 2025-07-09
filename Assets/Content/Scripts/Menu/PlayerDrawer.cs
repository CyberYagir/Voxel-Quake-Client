using Content.Scripts.Scriptable;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Menu
{
    public class PlayerDrawer : MonoBehaviour
    {
        [SerializeField] private Material playerMaterial;
        private PlayerConfigObject config;

        [Inject]
        private void Construct(PlayerConfigObject config)
        {
            this.config = config;
            this.config.OnConfigChanged.AddListener(UpdateColor);
            UpdateColor();
        }

        private void UpdateColor()
        {
            print(config.PlayerColor);
            if (ColorUtility.TryParseHtmlString(config.PlayerColor, out var color))
            {
                playerMaterial.SetColor("_BaseColor", color);
            }
        }
    }
}
