using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib.TimePeriodLogic;
using Integration.ChannelGrabberConfig;

namespace Integration.ChannelGrabberFrontend {

	class OrdersAggregatorFactory
		: DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<AInternalOrderItem>, AInternalOrderItem, FunctionType>
	{

		public override
			DataAggregatorBase<ReceivedDataListTimeDependentInfo<AInternalOrderItem>, AInternalOrderItem, FunctionType>
		CreateDataAggregator(
			ReceivedDataListTimeDependentInfo<AInternalOrderItem> data,
			ICurrencyConvertor currencyConverter
		) {
			return new OrdersAggregator(data, currencyConverter);
		} // CreateDataAggregator

	} // class OrdersAggregatorFactory

} // namespace
