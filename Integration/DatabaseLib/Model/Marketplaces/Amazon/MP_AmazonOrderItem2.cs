using System;
using EZBob.DatabaseLib.Common;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Marketplaces.Amazon
{
	public class MP_AmazonOrderItem2
	{
		public MP_AmazonOrderItem2()
		{
			PaymentsInfo = new HashedSet<MP_AmazonOrderItem2Payment>();
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
		public virtual ISet<MP_AmazonOrderItem2Payment> PaymentsInfo { get; set; }
		public virtual ISet<MP_AmazonOrderItemDetail> OrderItemDetails { get; set; }
	}
}