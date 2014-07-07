namespace EZBob.DatabaseLib.Model.Marketplaces.Amazon
{
	using FluentNHibernate.Mapping;

	public class MP_AmazonOrderItemDetailMap : ClassMap<MP_AmazonOrderItemDetail>
	{
		public MP_AmazonOrderItemDetailMap()
		{
			Table( "MP_AmazonOrderItemDetail" );
			Id( x => x.Id );
			References( x => x.OrderItem, "OrderItemId" );
			Map( x => x.OrderItemId, "AmazonOrderItemId" );
			Map( x => x.SellerSKU );
			Map( x => x.ASIN);
			Map( x => x.GiftMessageText );
			Map( x => x.GiftWrapLevel );
			Map( x => x.QuantityOrdered );
			Map( x => x.QuantityShipped );
			Map( x => x.Title );
			Component( x => x.CODFee, m =>
			                          	{
			                          		m.Map( x => x.CurrencyCode, "CODFeeCurrency" ).Length( 10 );
			                          		m.Map( x => x.Value, "CODFeePrice" );
			                          	} );			
			Component( x => x.CODFeeDiscount, m =>
			                            {
			                                m.Map( x => x.CurrencyCode, "CODFeeDiscountCurrency" ).Length( 10 );
			                                m.Map( x => x.Value, "CODFeeDiscountPrice" );
			                            } );			
			
			Component( x => x.GiftWrapPrice, m =>
			                            {
											m.Map( x => x.CurrencyCode, "GiftWrapPriceCurrency" ).Length( 10 );
			                                m.Map( x => x.Value, "GiftWrapPrice" );
			                            } );	
		
			Component( x => x.GiftWrapTax, m =>
			                            {
			                               	m.Map( x => x.CurrencyCode, "GiftWrapTaxCurrency" ).Length( 10 );
			                               	m.Map( x => x.Value, "GiftWrapTaxPrice" );
			                            } );
			
			Component( x => x.ItemPrice, m =>
			                            {
			                             	m.Map( x => x.CurrencyCode, "ItemPriceCurrency" ).Length( 10 );
			                             	m.Map( x => x.Value, "ItemPrice" );
			                            } );
			
			Component( x => x.ItemTax, m =>
			                           	{
			                           		m.Map( x => x.CurrencyCode, "ItemTaxCurrency" ).Length( 10 );
			                           		m.Map( x => x.Value, "ItemTaxPrice" );
			                           	} );			
			
			Component( x => x.PromotionDiscount, m =>
			                            {
			                                m.Map( x => x.CurrencyCode, "PromotionDiscountCurrency" ).Length( 10 );
			                                m.Map( x => x.Value, "PromotionDiscountPrice" );
			                            } );			
			
			Component( x => x.ShippingDiscount, m =>
										{
											m.Map( x => x.CurrencyCode, "ShippingDiscountCurrency" ).Length( 10 );
											m.Map( x => x.Value, "ShippingDiscountPrice" );
										} );
			
			Component( x => x.ShippingPrice, m =>
										{
											m.Map( x => x.CurrencyCode, "ShippingPriceCurrency" ).Length( 10 );
											m.Map( x => x.Value, "ShippingPrice" );
										} );

			Component( x => x.ShippingTax, m =>
			                            {
			                               	m.Map( x => x.CurrencyCode, "ShippingTaxCurrency" ).Length( 10 );
			                               	m.Map( x => x.Value, "ShippingTaxPrice" );
			                            } );

			HasMany( x => x.OrderItemCategories )
				.Cascade.AllDeleteOrphan()
				.Fetch.Join()
				.Inverse().KeyColumn( "AmazonOrderItemDetailId" );
			
		}
	}
}