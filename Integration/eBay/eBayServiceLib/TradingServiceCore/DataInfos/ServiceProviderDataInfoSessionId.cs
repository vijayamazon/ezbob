namespace EzBob.eBayServiceLib.TradingServiceCore.DataInfos
{
	public class ServiceProviderDataInfoSessionId : DataInfoString
	{
		public ServiceProviderDataInfoSessionId( string value )
			: base( value )
		{
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.SessionId; }
		}
	}
}