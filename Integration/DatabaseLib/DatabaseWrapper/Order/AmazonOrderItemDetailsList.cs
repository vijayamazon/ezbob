using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.Model.Database;
using EzBob.CommonLib.ReceivedDataListLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	public class AmazonOrderItemDetailsList : ReceivedDataListBase<AmazonOrderItemDetailInfo>
	{
		public AmazonOrderItemDetailsList(DateTime submittedDate, IEnumerable<AmazonOrderItemDetailInfo> collection = null) 
			: base(submittedDate, collection)
		{
		}
	}

	public class AmazonOrderItemDetailInfo
	{
		public string ASIN { get; set; }
		public AmountInfo CODFee { get; set; }
		public AmountInfo CODFeeDiscount { get; set; }
		public string GiftMessageText { get; set; }
		public string GiftWrapLevel { get; set; }
		public AmountInfo GiftWrapPrice { get; set; }
		public AmountInfo GiftWrapTax { get; set; }
		public AmountInfo ItemPrice { get; set; }
		public AmountInfo ItemTax { get; set; }
		public string OrderItemId { get; set; }
		public AmountInfo PromotionDiscount { get; set; }
		//public PromotionIdList PromotionIds { get; set; }
		public decimal QuantityOrdered { get; set; }
		public decimal QuantityShipped { get; set; }
		public string SellerSKU { get; set; }
		public AmountInfo ShippingDiscount { get; set; }
		public AmountInfo ShippingPrice { get; set; }
		public AmountInfo ShippingTax { get; set; }
		public string Title { get; set; }

		public MP_EbayAmazonCategory[] Categories { get; set; }
	}
}