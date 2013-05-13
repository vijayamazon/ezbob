namespace YodleeLib.config
{
    public interface IYodleeMarketPlaceConfig
    {
        string cobrandId { get; }
        string applicationId { get; }
        string username { get; }
        string password { get; }
        string soapServer { get; }
        string tncVersion { get; }
        string BridgetApplicationID { get; }
        string ApplicationKey { get; }
        string ApplicationToken { get; }
        string AddAccountURL { get; }
        string EditAccountURL { get; }
    }
}