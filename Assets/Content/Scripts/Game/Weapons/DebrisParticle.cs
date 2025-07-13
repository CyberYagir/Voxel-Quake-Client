using Content.Scripts.Game.Scriptable;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game.Weapons
{
    public class DebrisParticle : MonoBehaviour
    {
        private static readonly int ColorA = Shader.PropertyToID("_ColorA");
        private static readonly int ColorB = Shader.PropertyToID("_ColorB");
        [SerializeField] private ParticleSystem particleSystem;
        private MaterialListObject materialListObject;

        [Inject]
        private void Construct(MaterialListObject materialListObject)
        {
            this.materialListObject = materialListObject;
        }

        public void Init(byte materialIndex, int count)
        {
            var main = particleSystem.main;
            var mat = materialListObject.GetVoxelByMaterial(materialIndex).Material;
            main.startColor =
                new ParticleSystem.MinMaxGradient(mat.GetColor(ColorA), mat.GetColor(ColorB));

            var emission = particleSystem.emission;

            var burst = emission.GetBurst(0);
            burst.count = new ParticleSystem.MinMaxCurve(count/2f);

            emission.SetBurst(0, burst);
            
            particleSystem.Play(true);
            
            Destroy(gameObject, 3f);
        }
    }
}
