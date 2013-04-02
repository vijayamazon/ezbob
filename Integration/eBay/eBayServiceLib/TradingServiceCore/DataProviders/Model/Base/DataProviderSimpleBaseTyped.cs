using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base
{
	public abstract class DataProviderSimpleBaseTyped<TRezult, TParams> : DataProviderTokenDependentBaseTyped<TRezult, TParams>
		where TRezult : IResultDataInfo
		where TParams : IParamsDataInfo
	{
		protected DataProviderSimpleBaseTyped(IEbayServiceProvider serviceProvider)
			: base( new DataProviderCreationInfo(serviceProvider) )
		{

		}
	}
}