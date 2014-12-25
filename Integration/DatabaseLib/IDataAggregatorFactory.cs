namespace EZBob.DatabaseLib {
	using EzBob.CommonLib.TimePeriodLogic;

	public interface IDataAggregatorFactory<T, TItem, TEnum>
		where T : ReceivedDataListTimeDependentInfo<TItem>
		where TItem : class {
		DataAggregatorBase<T, TItem, TEnum> CreateDataAggregator(T data, ICurrencyConvertor currencyConverter);
	}

	public abstract class DataAggregatorFactoryBase<T, TItem, TEnum> : IDataAggregatorFactory<T, TItem, TEnum>
		where T : ReceivedDataListTimeDependentInfo<TItem>
		where TItem : class {
		public abstract DataAggregatorBase<T, TItem, TEnum> CreateDataAggregator(T data, ICurrencyConvertor currencyConverter);
	}
}