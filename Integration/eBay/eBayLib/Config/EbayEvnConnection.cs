using EzBob.eBayServiceLib.Common;
using Scorto.Configuration;

namespace EzBob.eBayLib.Config
{
	public class EbayEvnConnection : ConfigurationRoot, IEbayMarketplaceTypeConnection
    {
        public ServiceEndPointType ServiceType
        {
            get { return GetValue<ServiceEndPointType>("ServiceType"); }
        }

        public string DevId
        {
            get { return GetValue<string>("DevId"); }
        }

        public string AppId
        {
            get { return GetValue<string>("AppId"); }
        }

        public string CertId
        {
            get { return GetValue<string>("CertId"); }
        }

        public string RuName
        {
            get { return GetValue<string>("RuName"); }
        }
		
    }
}