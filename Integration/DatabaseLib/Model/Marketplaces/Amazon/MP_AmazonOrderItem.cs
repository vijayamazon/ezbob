namespace EZBob.DatabaseLib.Model.Marketplaces.Amazon
{
	using System;
	using Common;
	using Iesi.Collections.Generic;

	public class MP_AmazonOrderItem
	{
		public MP_AmazonOrderItem()
		{
			PaymentsInfo = new HashedSet<MP_AmazonOrderItemPayment>();
			OrderItemDetails = new HashedSet<MP_AmazonOrderItemDetail>();
		}

		public virtual int Id { get; set; }

		public virtual MP_AmazonOrder Order { get; set; }

		public virtual string OrderId { get; set; }
		public virtual string SellerOrderId { get; set; }
		public virtual DateTime? PurchaseDate { get; set; }
		public virtual DateTime? LastUpdateDate { get; set; }
		public virtual string OrderStatus { get; set; }
		public virtual AmountInfo OrderTotal { get; set; }
		public virtual int? NumberOfItemsShipped { get; set; }
		public virtual int? NumberOfItemsUnshipped { get; set; }
		public virtual ISet<MP_AmazonOrderItemPayment> PaymentsInfo { get; set; }
		public virtual ISet<MP_AmazonOrderItemDetail> OrderItemDetails { get; set; }
	}
}