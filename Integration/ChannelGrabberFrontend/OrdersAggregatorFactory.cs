using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib.TimePeriodLogic;
using Integration.ChannelGrabberConfig;

namespace Integration.ChannelGrabberFrontend {
	#region class OrdersAggregatorFactory

	class OrdersAggregatorFactory
		: DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<AInternalOrderItem>, AInternalOrderItem, FunctionType>
	{
		#region public

		#region method CreateDataAggregator

		public override
			DataAggregatorBase<ReceivedDataListTimeDependentInfo<AInternalOrderItem>, AInternalOrderItem, FunctionType>
		CreateDataAggregator(
			ReceivedDataListTimeDependentInfo<AInternalOrderItem> data,
			ICurrencyConvertor currencyConverter
		) {
			return new OrdersAggregator(data, currencyConverter);
		} // CreateDataAggregator

		#endregion method CreateDataAggregator

		#endregion public
	} // class OrdersAggregatorFactory

	#endregion class OrdersAggregatorFactory
} // namespace
