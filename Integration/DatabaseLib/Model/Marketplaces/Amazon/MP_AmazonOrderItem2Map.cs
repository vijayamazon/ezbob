using EZBob.DatabaseLib.DatabaseWrapper.Order;
using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
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
			Map(x => x.FulfillmentChannel);
			Map(x => x.SalesChannel);
			Map(x => x.OrderChannel);
			Map(x => x.ShipServiceLevel);
			Component(x => x.OrderTotal, m =>
			    {
			        m.Map(x => x.CurrencyCode, "OrderTotalCurrency").Length(50);
			        m.Map(x => x.Value, "OrderTotal");
			    });
			Map(x => x.PaymentMethod);
			Map(x => x.BuyerName);
			Map(x => x.ShipmentServiceLevelCategory);
			Map(x => x.BuyerEmail);
			Map(x => x.NumberOfItemsShipped);
			Map(x => x.NumberOfItemsUnshipped);
			Map(x => x.MarketplaceId);

			Component( x => x.ShipmentAddress, a =>
				{
					a.Map( x => x.AddressLine1, "ShipAddress1" );
					a.Map( x => x.AddressLine2, "ShipAddress2" );
					a.Map( x => x.AddressLine3, "ShipAddress3" );
					a.Map( x => x.City, "ShipCity" );
					a.Map( x => x.CountryCode, "ShipCountryCode" );
					a.Map( x => x.County, "ShipCounty" );
					a.Map( x => x.District, "ShipDistrict" );
					a.Map( x => x.Name, "ShipName" );
					a.Map( x => x.Phone, "ShipPhone" );
					a.Map( x => x.PostalCode, "PostalCode" );
					a.Map( x => x.StateOrRegion, "StateOrRegion" );
				} );

			HasMany(x => x.PaymentsInfo).
				KeyColumn("OrderItem2Id")
				.Cascade.All();

			HasMany( x => x.OrderItemDetails ).
				KeyColumn( "OrderItem2Id" )
				.Cascade.All();
		}
	}
}