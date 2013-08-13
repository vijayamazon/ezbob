using System;
using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
	[Obsolete]
	public class MP_PayPalTransactionItemMap : ClassMap<MP_PayPalTransactionItem>
	{
		public MP_PayPalTransactionItemMap()
		{
			Table( "MP_PayPalTransactionItem" );
			Id( x => x.Id );
			References( x => x.Transaction, "TransactionId" );
			Component( x => x.FeeAmount, m =>
			                                    	{
														m.Map( x => x.CurrencyCode, "FeeAmountCurrency" ).Length( 50 );
														m.Map( x => x.Value, "FeeAmountAmount" );
			                                    	} );

			Component( x => x.GrossAmount, m =>
			                              	{
												m.Map( x => x.CurrencyCode, "GrossAmountCurrency" ).Length( 50 );
												m.Map( x => x.Value, "GrossAmountAmount" );
			                              	} );

			Component( x => x.NetAmount, m =>
			                            	{
												m.Map( x => x.CurrencyCode, "NetAmountCurrency" ).Length( 50 );
												m.Map( x => x.Value, "NetAmountAmount" );
			                            	} );

			Map( x => x.Created ).CustomType<UtcDateTimeType>();
			Map( x => x.TimeZone );
			Map( x => x.Type );
			Map( x => x.Status );
			Map( x => x.Payer );
			Map( x => x.PayerDisplayName );
			Map( x => x.PayPalTransactionId );
		}
	}
}