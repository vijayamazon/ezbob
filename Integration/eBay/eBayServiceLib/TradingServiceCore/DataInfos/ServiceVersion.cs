namespace EzBob.eBayServiceLib.TradingServiceCore.DataInfos
{
	public class ServiceVersion : DataInfoString
	{
		public ServiceVersion( string ver )
			: base( ver )
		{
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.ApiVersion; }
		}
	}
}