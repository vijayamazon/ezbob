using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.DatabaseWrapper.FunctionValues;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib
{
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
	}
}