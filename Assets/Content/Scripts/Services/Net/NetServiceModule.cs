namespace Content.Scripts.Services.Net
{
    [System.Serializable]
    public abstract class NetServiceModule
    {
        protected NetService netService;

        public int LocalPlayerID => netService.Peer.RemoteId;
        
        protected NetServiceModule(NetService netService)
        {
            this.netService = netService;
        }

        public virtual void Dispose()
        {
            
        }
    }
}