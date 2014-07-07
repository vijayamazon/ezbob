namespace EZBob.DatabaseLib.Model.Marketplaces.Amazon
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class MP_AmazonOrderItemMap : ClassMap<MP_AmazonOrderItem>
	{
		public MP_AmazonOrderItemMap()
		{
			Table("MP_AmazonOrderItem");
			Id(x => x.Id);
			References(x => x.Order, "AmazonOrderId");
			Map(x => x.OrderId);
			Map(x => x.SellerOrderId);
			Map(x => x.PurchaseDate).CustomType<UtcDateTimeType>();
			Map(x => x.LastUpdateDate).CustomType<UtcDateTimeType>();
			Map( x => x.OrderStatus );
			Component(x => x.OrderTotal, m =>
			    {
			        m.Map(x => x.CurrencyCode, "OrderTotalCurrency").Length(50);
			        m.Map(x => x.Value, "OrderTotal");
			    });
			Map(x => x.NumberOfItemsShipped);
			Map(x => x.NumberOfItemsUnshipped);

			HasMany(x => x.PaymentsInfo).
				KeyColumn("OrderItemId")
				.Cascade.All();

			HasMany( x => x.OrderItemDetails ).
				KeyColumn( "OrderItemId" )
				.Cascade.All();
		}
	}
}