using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos
{
	public interface IResultDataInfo
	{
		int ErrorCount { get; }
		bool HasError { get; }
		ErrorInfo[] Errors { get; }
	}
}