using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib
{
	public interface IDataAggregatorFactory<T, TItem, TEnum>
		where T : ReceivedDataListTimeDependentInfo<TItem>
		where TItem : class, ITimeRangedData
	{
		DataAggregatorBase<T, TItem, TEnum> CreateDataAggregator( T data, ICurrencyConvertor currencyConverter );
	}

	public abstract class DataAggregatorFactoryBase<T, TItem, TEnum> : IDataAggregatorFactory<T, TItem, TEnum>
		where T : ReceivedDataListTimeDependentInfo<TItem>
		where TItem : class, ITimeRangedData
	{
		protected DataAggregatorFactoryBase()
		{
		}

		public abstract DataAggregatorBase<T, TItem, TEnum> CreateDataAggregator(T data, ICurrencyConvertor currencyConverter);
	}
}