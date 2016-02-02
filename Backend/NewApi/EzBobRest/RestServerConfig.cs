namespace EzBobRest
{
    public class RestServerConfig
    {
        public string ServerAddress { get; private set; }
        public string ServiceAddress { get; private set; }
        public int SendReceiveTaskTimeoutMilis { get; private set; }
    }
}
