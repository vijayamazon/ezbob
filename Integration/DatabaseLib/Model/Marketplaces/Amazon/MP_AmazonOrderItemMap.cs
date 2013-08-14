using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Marketplaces.Amazon
{
	public class MP_AmazonOrderItemMap : ClassMap<MP_AmazonOrderItem>
	{
		public MP_AmazonOrderItemMap()
		{
			Table("MP_AmazonOrderItem");
			Id(x => x.Id);
			References(x => x.Order, "AmazonOrderId");
			Map( x => x.OrderId);
			Map( x => x.OrderItemId);
			Map( x => x.PurchaseDate ).CustomType<UtcDateTimeType>();
			Map( x => x.PaymentsDate ).CustomType<UtcDateTimeType>();
			Map( x => x.BayerEmail);
			Map( x => x.BayerName);
			Map( x => x.BayerPhone);
			Map( x => x.Sku);
			Map( x => x.ProductName);
			Map( x => x.QuantityPurchased);
			Map( x => x.Currency);
			Map( x => x.ItemPrice);
			Map( x => x.ItemTax);

			Map( x => x.RecipientName);
			Map( x => x.SalesChennel);

			Map( x => x.ShipStreet);
			Map( x => x.ShipStreet1);
			Map( x => x.ShipStreet2);
			Map( x => x.ShipCityName);
			Map( x => x.ShipStateOrProvince);
			Map( x => x.ShipCountryName);
			Map( x => x.ShipPostalCode);
			Map( x => x.ShipPhone);
			Map( x => x.ShipRecipient);
			Map( x => x.ShipingPrice);
			Map( x => x.ShipingTax);
			Map( x => x.ShipServiceLevel);

			Map( x => x.DeliveryStartDate ).CustomType<UtcDateTimeType>();
			Map( x => x.DeliveryEndDate ).CustomType<UtcDateTimeType>();
			Map( x => x.DeliveryTimeZone);
			Map( x => x.DeliveryInstructions );
		}
	}
}