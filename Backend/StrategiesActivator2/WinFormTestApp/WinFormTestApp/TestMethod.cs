namespace WinFormTestApp
{
   static  class TestMethod
    {

		//private static BiztalkServiceNS.Header header = new BiztalkServiceNS.Header();
		//private static BiztalkServiceNS.ServiceClient client = new BiztalkServiceNS.ServiceClient();

		//public static string testCheckEntrust(string req, out string hdr, out BiztalkServiceNS.Header hdrObj)
		//{
		//	CheckEntrustRequest request = (CheckEntrustRequest)xmlHelper.DeserializeObject<CheckEntrustRequest>(req);
		//	CheckEntrustResponse response = client.CheckEntrust(out header, request);
		//	hdrObj = header;
		//	hdr = xmlHelper.SerializeObject<BiztalkServiceNS.Header>(header, name: "CheckEntrust");
		//	return xmlHelper.SerializeObject<CheckEntrustResponse>(response);

		//}

	   private static StasEzService.EzServiceClient client;
	   public static StasEzService.EzServiceClient CreateProxy(string endpointName)
		{
			switch (endpointName)
			{
				default:
					return client = GetStasServiceProxyInstance(endpointName);
			}
		}

		internal static StasEzService.EzServiceClient GetStasServiceProxyInstance(string endpointName)
		{
			//Get Client Endpoint from App.Config
			return new StasEzService.EzServiceClient(endpointName);

			//BasicHttpBinding binding = new BasicHttpBinding();
			//binding.SendTimeout = TimeSpan.FromMinutes(1);
			//binding.OpenTimeout = TimeSpan.FromMinutes(1);
			//binding.CloseTimeout = TimeSpan.FromMinutes(1);
			//binding.MaxBufferPoolSize = 524288;
			//binding.MaxBufferSize = 1000000;
			//binding.MaxReceivedMessageSize = 1000000;
			//binding.Name = "BassicHttpBinding_IService1";
			//binding.Namespace = "http://tepuri.org/";
			//binding.ProxyAddress = null;
			//binding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;

			//binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
			//binding.AllowCookies = false;
			//binding.BypassProxyOnLocal = false;
			//binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
			//binding.MessageEncoding = WSMessageEncoding.Text;
			//binding.TextEncoding = System.Text.Encoding.Unicode;
			//binding.TransferMode = TransferMode.Buffered;
			//binding.UseDefaultWebProxy = true;

			//binding.Security.Mode = BasicHttpSecurityMode.None;
			//binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
			//binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
			////binding.Security.Transport.Realm = "";
			//binding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Default;
			////binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.No;

			//return new FBNServiceNS.ServiceClient(binding,
			//  new EndpointAddress("http://192.168.234.96/IntegrationServices/Services.svc"));
			////return new FBNServiceNS.ServiceClient(binding,
			////    new EndpointAddress("http://41.223.146.69/IntegrationServices/Services.svc")); 

		}

    }
}
