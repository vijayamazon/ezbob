using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base
{
	public abstract class DataProviderTokenDependentBaseTyped<TRezult, TParams> : DataProviderTokenDependentBase, IDataProviderBaseTyped<TRezult, TParams>
		where TRezult : IResultDataInfo
		where TParams : IParamsDataInfo
	{
		protected DataProviderTokenDependentBaseTyped(DataProviderCreationInfo info) 
			: base(info)
		{
		}

		public abstract TRezult GetData( TParams param );
		
	}
}