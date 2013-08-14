using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Marketplaces.Amazon
{
	public class MP_AmazonOrderItem2Map : ClassMap<MP_AmazonOrderItem2>
	{
		public MP_AmazonOrderItem2Map()
		{
			Table("MP_AmazonOrderItem2");
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
				KeyColumn("OrderItem2Id")
				.Cascade.All();

			HasMany( x => x.OrderItemDetails ).
				KeyColumn( "OrderItem2Id" )
				.Cascade.All();
		}
	}
}