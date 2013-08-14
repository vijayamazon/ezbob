using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Common;
using EzBob.CommonLib.ReceivedDataListLogic;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	public enum AmazonOrdersList2ItemStatusType
	{
		Pending,
		Unshipped,
		PartiallyShipped,
		Shipped,
		Canceled,
		Unfulfillable,
	}

	public class AmazonOrdersList2 : ReceivedDataListTimeMarketTimeDependentBase<AmazonOrderItem2>
	{
		public AmazonOrdersList2(DateTime submittedDate, IEnumerable<AmazonOrderItem2> collection = null) 
			: base(submittedDate, collection)
		{
		}

		public override ReceivedDataListTimeDependentBase<AmazonOrderItem2> Create(DateTime submittedDate, IEnumerable<AmazonOrderItem2> collection)
		{
			return new AmazonOrdersList2(submittedDate, collection);
		}
	}

	public class AmazonOrderItem2 : TimeDependentRangedDataBase
	{
		public string AmazonOrderId { get; set; }
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
	        return string.Format("{0}, AmazonOrderId: {1}, OrderStatus: {2}, OrderTotal: {3}, PurchaseDate: {4}, NumberOfItemsShipped: {5}", base.ToString(), AmazonOrderId, OrderStatus, OrderTotal.Value, PurchaseDate, NumberOfItemsShipped);
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

		public string SubPaymentMethod { get; set; }
	}
	
}