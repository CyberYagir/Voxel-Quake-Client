using Content.Scripts.Game;
using Content.Scripts.Services;
using Content.Scripts.Services.Net;

namespace Content.Scripts
{
    public class ProjectInstaller : MonoBinder
    {
        public override void InstallBindings()
        {
            BindService<ScenesService>();
            BindService<NetService>();
        }
    }
}
