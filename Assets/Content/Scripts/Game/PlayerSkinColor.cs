using System.Collections.Generic;
using Content.Scripts.Services.Net;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game
{
    public class PlayerSkinColor : MonoBehaviour
    {
        [System.Serializable]
        public class MeshData
        {
            [SerializeField] private SkinnedMeshRenderer meshRenderer;
            [SerializeField] private List<int> subMeshes;

            public MeshData(SkinnedMeshRenderer meshRenderer1)
            {
                this.meshRenderer = meshRenderer1;
                subMeshes = new List<int>();
            }

            public List<int> SubMeshes => subMeshes;

            public SkinnedMeshRenderer MeshRenderer => meshRenderer;
        }

        [SerializeField] private NetObject netObject;
        [SerializeField] private Material baseMat;
        [SerializeField] private List<MeshData> meshes;


        [Inject]
        private void Construct(NetService netService)
        {
            if (!netObject.isMine)
            {
                var mat = new Material(baseMat);

                mat.SetColor("_BaseColor", netService.GetModule<NetServicePlayers>().GetPlayerColor(netObject.PeerID));

                for (int i = 0; i < meshes.Count; i++)
                {
                    var mats = meshes[i].MeshRenderer.sharedMaterials;

                    for (int j = 0; j < mats.Length; j++)
                    {
                        if (meshes[i].SubMeshes.Contains(j))
                        {
                            mats[j] = mat;
                        }
                    }

                    meshes[i].MeshRenderer.sharedMaterials = mats;
                }
            }
        }

        [Button]
        public void GetMeshes(Transform root)
        {
            var renderer = root.GetComponentsInChildren<SkinnedMeshRenderer>();

            meshes.Clear();


            for (int i = 0; i < renderer.Length; i++)
            {

                var d = new MeshData(renderer[i]);


                var mats = d.MeshRenderer.sharedMaterials;

                for (int j = 0; j < mats.Length; j++)
                {
                    if (mats[j] == baseMat)
                    {
                        d.SubMeshes.Add(j);
                    }
                }

                if (d.SubMeshes.Count > 0)
                {
                    meshes.Add(d);
                }
            }
        }
    }
}
