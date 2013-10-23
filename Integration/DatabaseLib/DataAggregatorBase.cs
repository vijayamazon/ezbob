namespace EZBob.DatabaseLib
{
	using System;
	using System.Collections.Generic;
	using DatabaseWrapper.FunctionValues;
	using EzBob.CommonLib.TimePeriodLogic;

	public abstract class DataAggregatorBase<T, TItem, TEnum>
		where T : ReceivedDataListTimeDependentInfo<TItem>
		where TItem : class
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
				Value = value
			};
		}

		public object CalculateAggregatorValue( TEnum type, IEnumerable<TItem> data )
		{
			return InternalCalculateAggregatorValue( type, data );
		}

		protected abstract object InternalCalculateAggregatorValue( TEnum type, IEnumerable<TItem> data );
	}
}
