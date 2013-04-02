using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EZBob.DatabaseLib.DatabaseWrapper.Transactions;
using EzBob.CommonLib.TimePeriodLogic;
using EzBob.PayPalDbLib;

namespace EzBob.PayPal
{
	internal class PayPalTransactionAgregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<PayPalTransactionItem>, PayPalTransactionItem, PayPalDatabaseFunctionType>
	{
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<PayPalTransactionItem>, PayPalTransactionItem, PayPalDatabaseFunctionType> CreateDataAggregator( ReceivedDataListTimeDependentInfo<PayPalTransactionItem> data, ICurrencyConvertor currencyConverter )
		{
			return new PayPalTransactionAgregator( data, currencyConverter );
		}


	}

	internal class PayPalTransactionAgregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<PayPalTransactionItem>, PayPalTransactionItem, PayPalDatabaseFunctionType>
	{
		public PayPalTransactionAgregator( ReceivedDataListTimeDependentInfo<PayPalTransactionItem> data, ICurrencyConvertor currencyConverter )
			: base( data, currencyConverter )
		{

		}

		protected override object InternalCalculateAggregatorValue( PayPalDatabaseFunctionType type, IEnumerable<PayPalTransactionItem> data )
		{
			switch ( type )
			{
				case PayPalDatabaseFunctionType.TotalNetInPayments:
					return GetTotalNetInPayments( data );

				case PayPalDatabaseFunctionType.TotalNetOutPayments:
					return GetTotalNetOutPayments( data );

				case PayPalDatabaseFunctionType.TransactionsNumber:
					return GetTransactionsNumber( data );

				default:
					throw new NotImplementedException();
			}
		}

		private object GetTransactionsNumber( IEnumerable<PayPalTransactionItem> data )
		{
			return data.Count(t => t.Status == "Completed" && t.Type == "Payment" && t.NetAmount.Value > 0);
		}

		private object GetTotalNetOutPayments( IEnumerable<PayPalTransactionItem> data )
		{
			return data.Where( t => t.Status == "Completed" && t.Type == "Transfer" && t.NetAmount.Value > 0 )
				.Sum( t => CurrencyConverter.ConvertToBaseCurrency( t.NetAmount.CurrencyCode, t.NetAmount.Value, t.Created ).Value );
		}

		private object GetTotalNetInPayments( IEnumerable<PayPalTransactionItem> data )
		{
			return data.Where( t => t.Status == "Completed" && t.Type == "Payment" && t.NetAmount.Value > 0 )
				.Sum( t => CurrencyConverter.ConvertToBaseCurrency( t.NetAmount.CurrencyCode, t.NetAmount.Value, t.Created ).Value );
		}
	}
}