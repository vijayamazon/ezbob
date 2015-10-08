namespace TeamCityModels.Connection
{
    public interface IClientConnection
    {
        void Connect(string userName, string password);
        void ConnectAsGuest();
    }
}