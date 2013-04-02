namespace EzBob.eBayServiceLib.TradingServiceCore.DataInfos
{
	public class ServiceProviderDataInfoUserId : DataInfoString
	{
		public ServiceProviderDataInfoUserId( string value )
			: base( value )
		{
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.UserId; }
		}
	}
}