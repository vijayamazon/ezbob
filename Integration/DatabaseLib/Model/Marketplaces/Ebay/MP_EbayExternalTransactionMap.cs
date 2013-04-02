using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayExternalTransactionMap : ClassMap<MP_EbayExternalTransaction>
	{
		public MP_EbayExternalTransactionMap()
		{
			Table("MP_EbayExternalTransaction");
			Id(x => x.Id);
			References(x => x.OrderItem, "OrderItemId");
			Component( x => x.FeeOrCreditAmount, m =>
			{
				m.Map( x => x.CurrencyCode, "FeeOrCreditCurrency" ).Length( 50 );
				m.Map( x => x.Value, "FeeOrCreditPrice" );
			} );

			Component( x => x.PaymentOrRefundAmount, m =>
			{
				m.Map( x => x.CurrencyCode, "PaymentOrRefundACurrency" ).Length( 50 );
				m.Map( x => x.Value, "PaymentOrRefundAPrice" );
			} );
			Map( x => x.TransactionTime );
			Map( x => x.TransactionID );
		}
	}
}