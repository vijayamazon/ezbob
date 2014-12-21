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
				case eBayDatabaseFunctionType.TopCategories:
			        var agg = new TopCategoriesAggregator();
                    return agg.GetTopCategories(ordersTeraPeak);

                case eBayDatabaseFunctionType.NumOfOrders:
                    return GetNumOfOrders(orders, ordersTeraPeak);

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
					return GetCancelledOrdersCount(orders, ordersTeraPeak);

				case eBayDatabaseFunctionType.OrdersCancellationRate:
                    var canceled = GetCancelledOrdersCount(orders, ordersTeraPeak);
                    int numOfOrder = GetNumOfOrders(orders, ordersTeraPeak);
                    return numOfOrder == 0 ? 0 : canceled / ((double)numOfOrder);

				case eBayDatabaseFunctionType.TotalItemsOrdered:
					return GetTotalItemsOrdered(orders, ordersTeraPeak);

				case eBayDatabaseFunctionType.TotalSumOfOrders:
					return GetEbayValue<double>(functionType, orders) + GetTotalSumOfOrders( ordersTeraPeak );

				case eBayDatabaseFunctionType.TotalSumOfOrdersAnnualized:
					double totalSumOfOrders = GetTotalSumOfOrders(ordersTeraPeak);
					var receivedDataListTimeDependentInfo = orders as ReceivedDataListTimeDependentInfo<MixedReceivedDataItem>;
					double annualizedSum = AnnualizeHelper.AnnualizeSum(receivedDataListTimeDependentInfo.TimePeriodType, receivedDataListTimeDependentInfo.SubmittedDate, totalSumOfOrders);
					return GetEbayValue<double>(functionType, orders) + annualizedSum;

				default:
					throw new NotImplementedException();
			}
		}

	    private int GetNumOfOrders(IEnumerable<MixedReceivedDataItem> orders, IEnumerable<TeraPeakDatabaseSellerDataItem> ordersTeraPeak)
	    {
	        return GetEbayValue<int>(eBayDatabaseFunctionType.NumOfOrders, orders) + GetShipedOrdersCount(ordersTeraPeak);
	    }

	    private int GetTotalItemsOrdered(IEnumerable<MixedReceivedDataItem> orders, IEnumerable<TeraPeakDatabaseSellerDataItem> ordersTeraPeak)
	    {
            return GetEbayValue<int>(eBayDatabaseFunctionType.TotalItemsOrdered, orders) + GetTotalItemsOrdered(ordersTeraPeak);
	    }

	    private int GetCancelledOrdersCount(IEnumerable<MixedReceivedDataItem> orders, IEnumerable<TeraPeakDatabaseSellerDataItem> ordersTeraPeak)
	    {
            return GetEbayValue<int>(eBayDatabaseFunctionType.CancelledOrdersCount, orders) + GetCancelledOrdersCount(ordersTeraPeak);
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
			return orders.Where( o => o.Data is TeraPeakDatabaseSellerDataItem ).Select( o => o.Data as TeraPeakDatabaseSellerDataItem ).ToList();
		}

		private int GetShipedOrdersCount( IEnumerable<TeraPeakDatabaseSellerDataItem> orders )
		{
			if ( !orders.Any() )
			{
				return 0;
			}
			return orders.Sum(o => o.Transactions.HasValue ? o.Transactions.Value : 0);
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

		private int GetCancelledOrdersCount( IEnumerable<TeraPeakDatabaseSellerDataItem> orders ) {
			if (orders == null)
				return 0;

			return orders.Where(oi => oi.Listings.HasValue && oi.Successful.HasValue).Sum(oi => oi.Listings.Value - oi.Successful.Value);
		}

	}
}
