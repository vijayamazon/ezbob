namespace EzBob.eBayServiceLib.TradingServiceCore.DataInfos
{
	public class ServiceProviderDataInfoCertId : DataInfoString
	{
		public ServiceProviderDataInfoCertId( string value )
			: base( value )
		{
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.CertificateId; }
		}
	}
}