namespace EZBob.DatabaseLib.Model.Marketplaces.Amazon
{
	using FluentNHibernate.Mapping;

	public class MP_AmazonOrderItemPaymentMap : ClassMap<MP_AmazonOrderItemPayment>
	{
		public MP_AmazonOrderItemPaymentMap()
		{
			Table( "MP_AmazonOrderItemPayment" );
			Id(x => x.Id);
			References(x => x.OrderItem, "OrderItemId");
			Map( x => x.SubPaymentMethod);
			Component( x => x.MoneyInfo, m =>
				{
					m.Map( x => x.CurrencyCode, "MoneyInfoCurrency" ).Length( 50 );
					m.Map( x => x.Value, "MoneyInfoAmount" );
				} );			
		}
	}
}