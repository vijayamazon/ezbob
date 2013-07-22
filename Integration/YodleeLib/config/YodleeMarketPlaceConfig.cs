namespace YodleeLib.config
{
    public class YodleeMarketPlaceConfig : IYodleeMarketPlaceConfig
    {
        public string cobrandId { get; set; }
        public string applicationId { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string soapServer { get; set; }
        public string tncVersion { get; set; }
        public string BridgetApplicationID { get; set; }
        public string ApplicationKey { get; set; }
        public string ApplicationToken { get; set; }
        public string AddAccountURL { get; set; }
        public string EditAccountURL { get; set; }
		public int AccountPoolSize { get; set; }
    }
}
