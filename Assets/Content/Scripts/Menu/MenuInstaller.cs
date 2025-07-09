using Content.Scripts.Game;
using Content.Scripts.Menu.Services;

namespace Content.Scripts.Menu
{
    public class MenuInstaller : MonoBinder
    {
        public override void InstallBindings()
        {
            BindService<ServersListService>();
            BindService<PlayerRenderService>();
        }
    }
}