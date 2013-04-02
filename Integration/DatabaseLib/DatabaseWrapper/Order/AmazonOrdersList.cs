using System;
using System.Collections.Generic;
using EzBob.CommonLib.ReceivedDataListLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	public class AmazonOrdersList : ReceivedDataListBase<AmazonOrderItem>
	{
		public AmazonOrdersList(DateTime submittedDate, IEnumerable<AmazonOrderItem> collection) 
			: base(submittedDate, collection)
		{

		}

		public AmazonOrdersList(DateTime submittedDate) : base(submittedDate)
		{

		}

		/*public override ReceivedDataListBase<AmazonOrderItem> Create(DateTime submittedDate, IEnumerable<AmazonOrderItem> collection)
		{
			return new AmazonOrdersList(submittedDate, collection);
		}*/
	}

	public class AmazonOrderItem
	{
		public string OrderId { get; set; }
		public string OrderItemId { get; set; }
		public DateTime? PurchaseDate { get; set; }
		public DateTime? PaymentsDate { get; set; }
		public string BayerEmail { get; set; }
		public string BayerName { get; set; }
		public string BayerPhone { get; set; }
		public string Sku { get; set; }
		public string ProductName { get; set; }
		public int QuantityPurchased { get; set; }
		public string Currency { get; set; }
		public double ItemPrice { get; set; }
		public double ItemTax { get; set; }
		
		public string RecipientName { get; set; }
		public string SalesChennel { get; set; }

		public string ShipStreet { get; set; }
		public string ShipStreet1 { get; set; }
		public string ShipStreet2 { get; set; }
		public string ShipCityName { get; set; }
		public string ShipStateOrProvince { get; set; }
		public string ShipCountryName { get; set; }
		public string ShipPostalCode { get; set; }
		public string ShipPhone { get; set; }
		public string ShipRecipient { get; set; }
		public double ShipingPrice { get; set; }
		public double ShipingTax { get; set; }
		public string ShipServiceLevel { get; set; }

		public DateTime? DeliveryStartDate { get; set; }
		public DateTime? DeliveryEndDate { get; set; }
		public string DeliveryTimeZone { get; set; }
		public string DeliveryInstructions { get; set; }
	}
}
