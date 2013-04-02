using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper.FunctionValues;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib
{
	public abstract class DataAggregatorBase<T, TItem, TEnum>
		where T : ReceivedDataListTimeDependentInfo<TItem>
		where TItem : class, ITimeRangedData
	{
		protected T Data { get; private set; }
		protected ICurrencyConvertor CurrencyConverter { get; private set; }

		protected DataAggregatorBase( T data, ICurrencyConvertor currencyConverter )
		{
			Data = data;
			CurrencyConverter = currencyConverter;
		}

		public IWriteDataInfo<TEnum> GetData( TEnum type, DateTime updatedDate )
		{
			var value =  InternalCalculateAggregatorValue( type, Data );

			return new WriteDataInfo<TEnum>
			{		
				UpdatedDate = updatedDate,
				FunctionType = type,
				TimePeriodType = Data.TimePeriodType,
				Value = value,
				CountMonthsFor = Data.CountMonths				
			};
		}

		public object CalculateAggregatorValue( TEnum type, IEnumerable<TItem> data )
		{
			return InternalCalculateAggregatorValue( type, data );
		}

		protected abstract object InternalCalculateAggregatorValue( TEnum type, IEnumerable<TItem> data );		
	}
}
