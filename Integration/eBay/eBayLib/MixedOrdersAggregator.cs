using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;
using EzBob.CommonLib.ReceivedDataListLogic;
using EzBob.CommonLib.TimePeriodLogic;
using EzBob.eBayDbLib;

namespace EzBob.eBayLib
{
	internal class MixedOrdersAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<MixedReceivedDataItem>, MixedReceivedDataItem, eBayDatabaseFunctionType>
	{
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<MixedReceivedDataItem>, MixedReceivedDataItem, eBayDatabaseFunctionType> CreateDataAggregator( ReceivedDataListTimeDependentInfo<MixedReceivedDataItem> data, ICurrencyConvertor currencyConverter )
		{
			return new MixedOrdersAggregator( data, currencyConverter );
		}
	}

	internal class MixedOrdersAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<MixedReceivedDataItem>, MixedReceivedDataItem, eBayDatabaseFunctionType>
	{
		private readonly DataAggregatorBase<ReceivedDataListTimeDependentInfo<EbayDatabaseOrderItem>, EbayDatabaseOrderItem, eBayDatabaseFunctionType> _EbayAggregator;
		
		public MixedOrdersAggregator( ReceivedDataListTimeDependentInfo<MixedReceivedDataItem> data, ICurrencyConvertor currencyConverter )
			: base( data, currencyConverter )
		{
			var factory = new EBayOrdersAgregatorFactory();
			_EbayAggregator = factory.CreateDataAggregator( null, currencyConverter );
			
		}

		protected override object InternalCalculateAggregatorValue( eBayDatabaseFunctionType functionType, IEnumerable<MixedReceivedDataItem> orders )
		{
			var ordersTeraPeak = GetTeraPeakOrders( orders );

			switch ( functionType )
			{
				case eBayDatabaseFunctionType.NumOfOrders:
					return GetEbayValue<int>( functionType, orders ) + GetShipedOrdersCount( ordersTeraPeak );
				
				case eBayDatabaseFunctionType.AverageItemsPerOrder:

					int totalOrdersCount = (int)InternalCalculateAggregatorValue( eBayDatabaseFunctionType.TotalItemsOrdered, orders );
					int shipedOrdersCount = (int)InternalCalculateAggregatorValue( eBayDatabaseFunctionType.NumOfOrders, orders );
					
					var newValueAverageItemsPerOrder = shipedOrdersCount == 0? 0: (int)Math.Round( totalOrdersCount/ (double)shipedOrdersCount, 0, MidpointRounding.AwayFromZero);;
					return newValueAverageItemsPerOrder;
					
				case eBayDatabaseFunctionType.AverageSumOfOrder:

					int shipedOrdersCount2 = (int)InternalCalculateAggregatorValue( eBayDatabaseFunctionType.NumOfOrders, orders );					
					double totalSum = (double)InternalCalculateAggregatorValue( eBayDatabaseFunctionType.TotalSumOfOrders, orders );
					
					var newValueAverageSum = shipedOrdersCount2 == 0 ? 0 : totalSum / shipedOrdersCount2;
					return newValueAverageSum;

				case eBayDatabaseFunctionType.CancelledOrdersCount:
					return GetEbayValue<int>( functionType, orders ) + GetCancelledOrdersCount( ordersTeraPeak );
					
				case eBayDatabaseFunctionType.OrdersCancellationRate:

					var canceled = GetEbayValue<int>(eBayDatabaseFunctionType.CancelledOrdersCount, orders) +
						GetCancelledOrdersCount(ordersTeraPeak);

					var numOfOrder = GetEbayValue<int>( eBayDatabaseFunctionType.NumOfOrders, orders ) +
						canceled +
						GetShipedOrdersCount( ordersTeraPeak );

					

					return numOfOrder == 0? 0:  canceled / (double)numOfOrder;

				case eBayDatabaseFunctionType.TotalItemsOrdered:
					return GetEbayValue<int>( functionType, orders ) + GetTotalItemsOrdered( ordersTeraPeak );

				case eBayDatabaseFunctionType.TotalSumOfOrders:
					return GetEbayValue<double>(functionType, orders) + GetTotalSumOfOrders( ordersTeraPeak );

				default:
					throw new NotImplementedException();
			}
		}

		private T GetEbayValue<T>( eBayDatabaseFunctionType funcType, IEnumerable<MixedReceivedDataItem> orders )
		{
			var rez = default(T);

			var ebayOrders = orders.Where( o => o.Data is EbayDatabaseOrderItem ).Select( o => o.Data as EbayDatabaseOrderItem );

			if ( ebayOrders.Any() )
			{
				var data = _EbayAggregator.CalculateAggregatorValue( funcType, ebayOrders );

				if ( data != null )
				{
					rez = (T)data;
				}
			}

			return rez;
		}

		private IEnumerable<TeraPeakDatabaseSellerDataItem>  GetTeraPeakOrders( IEnumerable<MixedReceivedDataItem> orders )
		{
			return orders.Where( o => o.Data is TeraPeakDatabaseSellerDataItem ).Select( o => o.Data as TeraPeakDatabaseSellerDataItem );
		}

		private int GetShipedOrdersCount( IEnumerable<TeraPeakDatabaseSellerDataItem> orders )
		{
			if ( !orders.Any() )
			{
				return 0;
			}
			return orders.Sum( o => o.Successful.HasValue? o.Successful.Value: 0 );
		}

		private int GetTotalItemsOrdered( IEnumerable<TeraPeakDatabaseSellerDataItem> orders )
		{
			if ( !orders.Any() )
			{
				return 0;
			}
			return orders.Sum( to => to.ItemsSold.HasValue? to.ItemsSold.Value: 0 );
		}

		private double GetTotalSumOfOrders( IEnumerable<TeraPeakDatabaseSellerDataItem> orders )
		{
			if ( !orders.Any() )
			{
				return 0;
			}
			return orders.Sum( to => to.Revenue.HasValue? to.Revenue.Value: 0 );
		}

		private int GetCancelledOrdersCount( IEnumerable<TeraPeakDatabaseSellerDataItem> orders )
		{
			if ( !orders.Any() )
			{
				return 0;
			}
			return orders.Sum( to =>
			                   	{
									if (!to.Successful.HasValue || !to.Transactions.HasValue)
			                   		{
			                   			return 0;
			                   		}

									return to.Transactions.Value - to.Successful.Value;
			                   	});
		}
		
		
	}
}