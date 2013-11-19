using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib.TimePeriodLogic;
using EzBob.eBayDbLib;

namespace EzBob.eBayLib
{

	internal class EBayOrdersAgregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<EbayDatabaseOrderItem>, EbayDatabaseOrderItem, eBayDatabaseFunctionType>
	{
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<EbayDatabaseOrderItem>, EbayDatabaseOrderItem, eBayDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<EbayDatabaseOrderItem> data, ICurrencyConvertor currencyConverter)
		{
			return new EBayOrdersAggregator( data, currencyConverter );
		}
	}

	internal class EBayOrdersAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<EbayDatabaseOrderItem>, EbayDatabaseOrderItem, eBayDatabaseFunctionType>
	{
		public EBayOrdersAggregator( ReceivedDataListTimeDependentInfo<EbayDatabaseOrderItem> ordersDataInfo, ICurrencyConvertor currencyConverter ) 
			: base(ordersDataInfo, currencyConverter)
		{
		}

		private int GetAverageItemsPerOrder( IEnumerable<EbayDatabaseOrderItem> orders )
		{
			var countItems = GetTotalItemsOrdered( orders );
			var countOrders = GetShipedOrdersCount( orders );

			return countOrders == 0 ? 0 : (int)Math.Round( countItems / (double)countOrders, MidpointRounding.AwayFromZero );
		}

		private int GetOrdersCount( IEnumerable<EbayDatabaseOrderItem> orders )
		{
			return orders.Count();
		}

		private int GetShipedOrdersCount( IEnumerable<EbayDatabaseOrderItem> orders )
		{
			return orders.Count( o => o.OrderStatus == EBayOrderStatusCodeType.Completed || 
				o.OrderStatus == EBayOrderStatusCodeType.Authenticated ||
				o.OrderStatus == EBayOrderStatusCodeType.Shipped);
		}

		private int GetCancelledOrdersCount( IEnumerable<EbayDatabaseOrderItem> orders )
		{
			return orders.Count( o => o.OrderStatus == EBayOrderStatusCodeType.Cancelled );
		}

		private double GetAverageSumOfOrder( IEnumerable<EbayDatabaseOrderItem> orders )
		{
			var sum = GetTotalSumOfOrders( orders );
			var count = GetShipedOrdersCount( orders );

			return count == 0 ? 0 : sum / count;
		}

		private double GetOrdersCancellationRate( IEnumerable<EbayDatabaseOrderItem> orders )
		{
			var canceled = GetCancelledOrdersCount( orders );
			var other = GetOrdersCount( orders );

			return other == 0 ? 0 : canceled / (double)other ;
		}

		private int GetTotalItemsOrdered( IEnumerable<EbayDatabaseOrderItem> orders )
		{
			var val =orders.Where( o => o.OrderStatus == EBayOrderStatusCodeType.Completed ||
				o.OrderStatus == EBayOrderStatusCodeType.Authenticated ||
				o.OrderStatus == EBayOrderStatusCodeType.Shipped ).Sum( o => o.TransactionData.Sum( t => t.QuantityPurchased) );

			return val.HasValue ? val.Value : 0;
		}

		private double GetTotalSumOfOrders(IEnumerable<EbayDatabaseOrderItem> orders)
		{
			return orders.Where(o => o.OrderStatus == EBayOrderStatusCodeType.Completed ||
				o.OrderStatus == EBayOrderStatusCodeType.Authenticated ||
				o.OrderStatus == EBayOrderStatusCodeType.Shipped).Sum(o => CurrencyConverter.ConvertToBaseCurrency(o.Total.CurrencyCode, o.Total.Value, o.CreatedTime).Value);
		}

		private double GetTotalSumOfOrdersAnnualized(IEnumerable<EbayDatabaseOrderItem> orders)
		{
			var ordersWithExtraInfo = orders as ReceivedDataListTimeDependentInfo<EbayDatabaseOrderItem>;
			if (ordersWithExtraInfo == null)
			{
				return 0;
			}
			
			double sum = GetTotalSumOfOrders(orders);
			return AnnualizeHelper.AnnualizeSum(ordersWithExtraInfo.TimePeriodType, ordersWithExtraInfo.SubmittedDate, sum);
		}

		protected override object InternalCalculateAggregatorValue( eBayDatabaseFunctionType functionType, IEnumerable<EbayDatabaseOrderItem> orders )
		{
			switch ( functionType )
			{
				case eBayDatabaseFunctionType.AverageItemsPerOrder:
					return GetAverageItemsPerOrder( orders );

				case eBayDatabaseFunctionType.AverageSumOfOrder:
					return GetAverageSumOfOrder( orders );

				case eBayDatabaseFunctionType.CancelledOrdersCount:
					return GetCancelledOrdersCount( orders );

				case eBayDatabaseFunctionType.NumOfOrders:
					return GetShipedOrdersCount( orders );

				case eBayDatabaseFunctionType.OrdersCancellationRate:
					return GetOrdersCancellationRate( orders );

				case eBayDatabaseFunctionType.TotalItemsOrdered:
					return GetTotalItemsOrdered(orders);

				case eBayDatabaseFunctionType.TotalSumOfOrders:
					return GetTotalSumOfOrders(orders);

				case eBayDatabaseFunctionType.TotalSumOfOrdersAnnualized:
					return GetTotalSumOfOrdersAnnualized(orders);

				default:
					throw new NotImplementedException();
			}
		}		
	}
}