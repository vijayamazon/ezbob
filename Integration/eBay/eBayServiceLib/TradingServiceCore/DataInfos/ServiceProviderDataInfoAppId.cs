namespace EzBob.eBayServiceLib.TradingServiceCore.DataInfos
{
	public class ServiceProviderDataInfoAppId : DataInfoString
	{
		public ServiceProviderDataInfoAppId( string value )
			: base( value )
		{
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.ApplicationId; }
		}
	}
}