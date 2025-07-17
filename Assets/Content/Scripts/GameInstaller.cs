using Content.Scripts.Game.Scriptable;
using Content.Scripts.Game.Services;
using Content.Scripts.Game.Voxels;
using Content.Scripts.Services.Net;
using UnityEngine;

namespace Content.Scripts
{
    public class GameInstaller : MonoBinder
    {
        [SerializeField] private MaterialListObject materialListObject;
        public override void InstallBindings()
        {
            var fabric = new PrefabSpawnerFabric(Container);
            Container.Bind<PrefabSpawnerFabric>().FromInstance(fabric).AsSingle().NonLazy();
            
            var netService = (NetService)Container.Resolve(typeof(NetService));
            netService.SetFabric(fabric);
            
            
            Container.Bind<MaterialListObject>().FromInstance(materialListObject).AsSingle().NonLazy();
        
            BindService<DataLoaderService>();
        
            BindService<VoxelVolume>();
            BindService<VoxelVolumeDrawer>();
            BindService<MapObjectsService>();
        
            BindService<MapLoaderService>();
        
            BindService<PlayerService>();
            BindService<ChatService>();


        }
    }
}
