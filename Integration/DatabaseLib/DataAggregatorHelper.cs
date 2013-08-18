namespace EZBob.DatabaseLib
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DatabaseWrapper.FunctionValues;
	using DatabaseWrapper.Order;
	using EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;
	using EzBob.CommonLib.TimePeriodLogic;
	using EzBob.CommonLib.ReceivedDataListLogic;

	public static class DataAggregatorHelper
	{
		private static IWriteDataInfo<TEnum> CreateWriteDataValue<T, TItem, TEnum>( DataAggregatorBase<T, TItem, TEnum> aggregator, TEnum functionType, DateTime updatedDate )
			where T : ReceivedDataListTimeDependentInfo<TItem>
			where TItem : class, ITimeRangedData
		{
			return aggregator.GetData( functionType, updatedDate );
		}

		public static IEnumerable<IWriteDataInfo<TEnum>> AggregateData<T, TItem, TEnum>(
			IDataAggregatorFactory<T, TItem, TEnum> dataAggregatorFactory,
			Dictionary<TimePeriodEnum, ReceivedDataListTimeDependentInfo<TItem>> timePeriodData,
			TEnum[] funcList,
			DateTime updatedDate,
			ICurrencyConvertor currencyConvertor
			)
			where T : ReceivedDataListTimeDependentInfo<TItem>
			where TItem : class, ITimeRangedData
		{
			var rez = new List<IWriteDataInfo<TEnum>>();

		    if (timePeriodData == null) return rez;

		    foreach (var valuePair in timePeriodData)
		    {
		        var allData = valuePair.Value as T;

		        DataAggregatorBase<T, TItem, TEnum> aggeregator = dataAggregatorFactory.CreateDataAggregator( allData, currencyConvertor );
		        var list = funcList.Select(func => CreateWriteDataValue(aggeregator, func, updatedDate));
		        rez.AddRange(list);
		    }
		    
            return rez;
		}

		public static Dictionary<TimePeriodEnum, ReceivedDataListTimeDependentInfo<T>> GetOrdersForPeriods<T>(ReceivedDataListTimeMarketTimeDependentBase<T> orders, Func<DateTime, List<T>, ReceivedDataListTimeMarketTimeDependentBase<T>> createListFunc)
			where T : TimeDependentRangedDataBase
		{
			Dictionary<TimePeriodEnum, List<T>> ordersByTimePeriod = GetOrdersByTimePeriod(orders);
			var res = new Dictionary<TimePeriodEnum, ReceivedDataListTimeDependentInfo<T>>();
			foreach (TimePeriodEnum period in ordersByTimePeriod.Keys)
			{
				if (ordersByTimePeriod[period].Count > 0)
				{
					res.Add(period, new ReceivedDataListTimeDependentInfo<T>(createListFunc(orders.SubmittedDate, ordersByTimePeriod[period]), period, 0));
				}
			}

			return res;
		}

		public static Dictionary<TimePeriodEnum, ReceivedDataListTimeDependentInfo<MixedReceivedDataItem>> GetOrdersForPeriodsEbay(MixedReceivedDataList orders)
		{
			Dictionary<TimePeriodEnum, List<MixedReceivedDataItem>> ordersByTimePeriod = GetOrdersByTimePeriodEbay(orders);
			var res = new Dictionary<TimePeriodEnum, ReceivedDataListTimeDependentInfo<MixedReceivedDataItem>>();
			foreach (TimePeriodEnum period in ordersByTimePeriod.Keys)
			{
				if (ordersByTimePeriod[period].Count > 0)
				{
					var listForPeriod = new MixedReceivedDataList(orders.SubmittedDate, ordersByTimePeriod[period]);
					res.Add(period, new ReceivedDataListTimeDependentInfo<MixedReceivedDataItem>(listForPeriod, period, 0));
				}
			}

			return res;
		}

		private static void PlaceOrderInRelevantPeriod<T>(DateTime timestamp, T item, Dictionary<TimePeriodEnum, DateTime> edges, Dictionary<TimePeriodEnum, List<T>> dict)
		{
			foreach (TimePeriodEnum period in edges.Keys)
			{
				if (timestamp >= edges[period])
				{
					dict[period].Add(item);
					return;
				}
			}
			
			dict[TimePeriodEnum.Lifetime].Add(item);
		}

		private static Dictionary<TimePeriodEnum, List<MixedReceivedDataItem>> GetOrdersByTimePeriodEbay(MixedReceivedDataList orders)
		{
			var ordersByTimePeriod = GetEmptyDictionary<MixedReceivedDataItem>();
			var edges = GetEdges(orders.SubmittedDate);

			foreach (MixedReceivedDataItem item in orders)
			{
				if (item.Data is TeraPeakDatabaseSellerDataItem)
				{
					var teraPeakDatabaseSellerDataItem = item.Data as TeraPeakDatabaseSellerDataItem;
					PlaceOrderInRelevantPeriod(teraPeakDatabaseSellerDataItem.StartDate, item, edges, ordersByTimePeriod);
				}
				else if (item.Data is EbayDatabaseOrderItem)
				{
					var ebayDatabaseOrderItem = item.Data as EbayDatabaseOrderItem;
					PlaceOrderInRelevantPeriod(ebayDatabaseOrderItem.RecordTime, item, edges, ordersByTimePeriod);
				}
			}

			var earliestFilledTimePeriod = TimePeriodEnum.Month;
			foreach (var period in ordersByTimePeriod.Keys)
			{
				if (ordersByTimePeriod[period].Count > 0)
				{
					earliestFilledTimePeriod = period;
				}
			}

			List<MixedReceivedDataItem> sumSoFar = null;
			foreach (var period in ordersByTimePeriod.Keys)
			{
				if (period <= earliestFilledTimePeriod)
				{
					if (sumSoFar != null)
					{
						ordersByTimePeriod[period].AddRange(sumSoFar);
					}
					sumSoFar = ordersByTimePeriod[period];
				}
			}

			return ordersByTimePeriod;
		}

		private static Dictionary<TimePeriodEnum, DateTime> GetEdges(DateTime timestamp)
		{
			DateTime monthAgo = timestamp.AddMonths(-1);
			var monthEdge = new DateTime(monthAgo.Year, monthAgo.Month, monthAgo.Day);
			DateTime month3Edge = GetStartOfMonth(timestamp, 3);
			DateTime month6Edge = GetStartOfMonth(timestamp, 6);
			DateTime month12Edge = GetStartOfMonth(timestamp, 12);
			DateTime month15Edge = GetStartOfMonth(timestamp, 15);
			DateTime month18Edge = GetStartOfMonth(timestamp, 18);
			DateTime month24Edge = GetStartOfMonth(timestamp, 24);

			var edges = new Dictionary<TimePeriodEnum, DateTime>
				{
					{TimePeriodEnum.Month, monthEdge},
					{TimePeriodEnum.Month3, month3Edge},
					{TimePeriodEnum.Month6, month6Edge},
					{TimePeriodEnum.Year, month12Edge},
					{TimePeriodEnum.Month15, month15Edge},
					{TimePeriodEnum.Month18, month18Edge},
					{TimePeriodEnum.Year2, month24Edge}
				};

			return edges;
		}

		private static Dictionary<TimePeriodEnum, List<T>> GetEmptyDictionary<T>()
		{
			return new Dictionary<TimePeriodEnum, List<T>>
				{
					{TimePeriodEnum.Month, new List<T>()},
					{TimePeriodEnum.Month3, new List<T>()},
					{TimePeriodEnum.Month6, new List<T>()},
					{TimePeriodEnum.Year, new List<T>()},
					{TimePeriodEnum.Month15, new List<T>()},
					{TimePeriodEnum.Month18, new List<T>()},
					{TimePeriodEnum.Year2, new List<T>()},
					{TimePeriodEnum.Lifetime, new List<T>()}
				};
		}
		
		private static Dictionary<TimePeriodEnum, List<T>> GetOrdersByTimePeriod<T>(ReceivedDataListTimeMarketTimeDependentBase<T> orders)
			where T : TimeDependentRangedDataBase
		{
			var ordersByTimePeriod = GetEmptyDictionary<T>();
			var edges = GetEdges(orders.SubmittedDate);

			foreach (T item in orders)
			{
				PlaceOrderInRelevantPeriod(item.RecordTime, item, edges, ordersByTimePeriod);
			}

			var earliestFilledTimePeriod = TimePeriodEnum.Month;
			foreach (var period in ordersByTimePeriod.Keys)
			{
				if (ordersByTimePeriod[period].Count > 0)
				{
					earliestFilledTimePeriod = period;
				}
			}

			List<T> sumSoFar = null;
			foreach (var period in ordersByTimePeriod.Keys)
			{
				if (period <= earliestFilledTimePeriod)
				{
					if (sumSoFar != null)
					{
						ordersByTimePeriod[period].AddRange(sumSoFar);
					}
					sumSoFar = ordersByTimePeriod[period];
				}
			}

			return ordersByTimePeriod;
		}

		private static DateTime GetStartOfMonth(DateTime relativeDate, int numOfMonth)
		{
			int newYear = relativeDate.Year;

			while (numOfMonth > 11)
			{
				numOfMonth -= 12;
				newYear--;
			}

			int newMonth = relativeDate.Month - numOfMonth + 1;
			if (relativeDate.Day == 1)
			{
				newMonth--;
			}

			if (newMonth < 1)
			{
				newYear--;
				newMonth = 12 + newMonth;
			}

			return new DateTime(newYear, newMonth, 1);
		}
	}
}