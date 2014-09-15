using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Common;
using EzBob.CommonLib.ReceivedDataListLogic;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	public enum AmazonOrdersList2ItemStatusType {
		Pending, //The order has been placed but payment has not been authorized. The order is not ready for shipment. Note that for orders with OrderType = Standard, the initial order status is Pending. For orders with OrderType = Preorder (available in JP only), the initial order status is PendingAvailability, and the order passes into the Pending status when the payment authorization process begins.
		Unshipped, //Payment has been authorized and order is ready for shipment, but no items in the order have been shipped.
		PartiallyShipped, //One or more (but not all) items in the order have been shipped. 
		Shipped, //All items in the order have been shipped.
		Canceled, //The order was canceled.
		Unfulfillable, //The order cannot be fulfilled. This state applies only to Amazon-fulfilled orders that were not placed on Amazon's retail web site.
		PendingAvailability, //This status is available for pre-orders only. The order has been placed, payment has not been authorized, and the release date of the item is in the future. The order is not ready for shipment. Note that Preorder is a possible OrderType value in Japan (JP) only.
		InvoiceUnconfirmed, //All items in the order have been shipped. The seller has not yet given confirmation to Amazon that the invoice has been shipped to the buyer. Note: This value is available only in China (CN).
		All //default
	}

	public class AmazonOrdersList : ReceivedDataListTimeMarketTimeDependentBase<AmazonOrderItem>
	{
		public AmazonOrdersList(DateTime submittedDate, IEnumerable<AmazonOrderItem> collection = null) 
			: base(submittedDate, collection)
		{
		}

		public override ReceivedDataListTimeDependentBase<AmazonOrderItem> Create(DateTime submittedDate, IEnumerable<AmazonOrderItem> collection)
		{
			return new AmazonOrdersList(submittedDate, collection);
		}
	}

	public class AmazonOrderItem : TimeDependentRangedDataBase
	{
		public string OrderId { get; set; }
		public string SellerOrderId { get; set; }
		public DateTime? PurchaseDate { get; set; }
		public DateTime? LastUpdateDate { get; set; }
		public AmazonOrdersList2ItemStatusType OrderStatus { get; set; }
		public AmountInfo OrderTotal { get; set; }
		public int? NumberOfItemsShipped { get; set; }
		public int? NumberOfItemsUnshipped { get; set; }
		public AmazonOrderItem2PaymentsInfoList PaymentsInfo { get; set; }
		public AmazonOrderItemDetailsList OrderedItemsList { get; set; }

		public override DateTime RecordTime
		{
			get { return PurchaseDate.Value; }
		}

	    public override string ToString()
	    {
	        return string.Format("{0}, AmazonOrderId: {1}, OrderStatus: {2}, OrderTotal: {3}, PurchaseDate: {4}, NumberOfItemsShipped: {5}", base.ToString(), OrderId, OrderStatus, OrderTotal.Value, PurchaseDate, NumberOfItemsShipped);
	    }
	}

	public class AmazonOrderItem2PaymentsInfoList : List<AmazonOrderItem2PaymentInfoListItem>
	{
		public AmazonOrderItem2PaymentsInfoList()
		{
		}

		public AmazonOrderItem2PaymentsInfoList(IEnumerable<AmazonOrderItem2PaymentInfoListItem> collection) 
			: base(collection)
		{
		}
	}

	public class AmazonOrderItem2PaymentInfoListItem
	{
		public AmountInfo MoneyInfo { get; set; }

		public string PaymentMethod { get; set; }
	}
	
}