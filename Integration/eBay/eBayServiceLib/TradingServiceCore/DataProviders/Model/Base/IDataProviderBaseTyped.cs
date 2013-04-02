using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base
{
	public interface IDataProviderBaseTyped<out TRezult, in TParams> : IDataProvider
		where TRezult : IResultDataInfo
		where TParams : IParamsDataInfo
	{
		TRezult GetData( TParams param );
	}
}