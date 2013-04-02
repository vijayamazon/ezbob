namespace EzBob.eBayServiceLib.TradingServiceCore.DataInfos
{
	public class ServiceProviderDataInfoToken : DataInfoString
	{
		public ServiceProviderDataInfoToken( string value )
			: base( value )
		{
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.Token; }
		}
	}
}