using System;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_AmazonOrderItem
	{
		public virtual int Id { get; set; }

		public virtual MP_AmazonOrder Order { get; set; }

		public virtual string OrderId { get; set; }
		public virtual string OrderItemId { get; set; }
		public virtual DateTime? PurchaseDate { get; set; }
		public virtual DateTime? PaymentsDate { get; set; }
		public virtual string BayerEmail { get; set; }
		public virtual string BayerName { get; set; }
		public virtual string BayerPhone { get; set; }
		public virtual string Sku { get; set; }
		public virtual string ProductName { get; set; }
		public virtual int QuantityPurchased { get; set; }
		public virtual string Currency { get; set; }
		public virtual double ItemPrice { get; set; }
		public virtual double ItemTax { get; set; }

		public virtual string RecipientName { get; set; }
		public virtual string SalesChennel { get; set; }

		public virtual string ShipStreet { get; set; }
		public virtual string ShipStreet1 { get; set; }
		public virtual string ShipStreet2 { get; set; }
		public virtual string ShipCityName { get; set; }
		public virtual string ShipStateOrProvince { get; set; }
		public virtual string ShipCountryName { get; set; }
		public virtual string ShipPostalCode { get; set; }
		public virtual string ShipPhone { get; set; }
		public virtual string ShipRecipient { get; set; }
		public virtual double ShipingPrice { get; set; }
		public virtual double ShipingTax { get; set; }
		public virtual string ShipServiceLevel { get; set; }

		public virtual DateTime? DeliveryStartDate { get; set; }
		public virtual DateTime? DeliveryEndDate { get; set; }
		public virtual string DeliveryTimeZone { get; set; }
		public virtual string DeliveryInstructions { get; set; }
	}
}