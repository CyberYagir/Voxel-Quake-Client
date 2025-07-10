using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Scriptable
{
    [CreateAssetMenu(menuName = "Create WeaponsConfigObject", fileName = "WeaponsConfigObject", order = 0)]
    public class WeaponsConfigObject : ScriptableObjectInstaller
    {
        [SerializeField] private List<WeaponDataObject> weaponsList;

        public List<WeaponDataObject> WeaponsList => weaponsList;
        
        
        public override void InstallBindings()
        {
            Container.Bind<WeaponsConfigObject>().FromInstance(this).AsSingle().NonLazy();
        }
    }
}
