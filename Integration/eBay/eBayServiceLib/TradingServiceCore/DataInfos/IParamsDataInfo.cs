namespace EzBob.eBayServiceLib.TradingServiceCore.DataInfos
{
	public interface IParamsDataInfo : IEbayDataInfo
	{
		bool HasData { get; }
		
	}
}