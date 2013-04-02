using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayTransactionMap : ClassMap<MP_EbayTransaction>
	{
		public MP_EbayTransactionMap()
		{
			Table("MP_EbayTransaction");
			Id(x => x.Id);
			References( x => x.OrderItem, "OrderItemId" );
			Component( x => x.TransactionPrice, m =>
			    {
			        m.Map( x => x.CurrencyCode, "PriceCurrency" ).Length(50);
			        m.Map( x => x.Value, "Price" );
			    } );
			Map( x => x.PaymentHoldStatus, "PaymentHoldStatus" ).Length( 50 );
			Map( x => x.PaymentMethodUsed, "" ).Length( 50 );
			Map( x => x.QuantityPurchased);
			Map( x => x.CreatedDate);

			Map( x => x.ItemID );
			Map( x => x.ItemPrivateNotes );
			Map( x => x.ItemSellerInventoryID );
			Map( x => x.ItemSKU );
			Map( x => x.eBayTransactionId );

			References( x => x.OrderItemDetail, "ItemInfoId" );
		}		
	}
}