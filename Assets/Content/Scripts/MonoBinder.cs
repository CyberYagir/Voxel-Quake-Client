using Zenject;

namespace Content.Scripts
{
    public class MonoBinder : MonoInstaller
    {
        protected void BindService<T>()
        {
            var service = GetComponentInChildren<T>();
            print(typeof(T).Name + " service <b>binded</b>");
            Container.Bind<T>().FromInstance(service).AsSingle().NonLazy();
            
        }
    }
}