namespace EzBob.eBayServiceLib.TradingServiceCore.DataInfos
{
	public class ServiceProviderDataInfoDevId : DataInfoString
	{
		public ServiceProviderDataInfoDevId( string value )
			: base( value )
		{
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.DevelorepId; }
		}
	}
}