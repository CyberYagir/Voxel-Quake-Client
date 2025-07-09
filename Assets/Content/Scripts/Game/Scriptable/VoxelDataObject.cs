using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Content.Scripts.Game.Scriptable
{
    [CreateAssetMenu(menuName = "Create VoxelDataObject", fileName = "VoxelDataObject", order = 0)]
    public class VoxelDataObject : ScriptableObject
    {
        [SerializeField] private Tile tile;
        [SerializeField] private Material material;
        [SerializeField] private int health;


        public int Health => health;

        public Material Material => material;

        public Tile Tile => tile;

        [Button, ShowIf("@tile == null || material == null")]
        public void Init()
        {
            CreateMaterial();
            CreateTile();
        }
        
        public void CreateMaterial()
        {
            
#if UNITY_EDITOR
            material = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
            material.name = "Material " + name;
            AssetDatabase.AddObjectToAsset(material, this);
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            
#endif
            
        }
        
        
        public void CreateTile()
        {
#if UNITY_EDITOR
            string guid = "64b71237cd0d2834cb44b31a81b54ef4";
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = "Tile " + name;
            tile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid));
            AssetDatabase.AddObjectToAsset(tile, this);
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            
#endif
            
        }
    }
}
