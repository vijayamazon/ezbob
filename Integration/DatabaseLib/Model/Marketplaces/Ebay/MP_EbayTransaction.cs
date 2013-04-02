using System;
using EZBob.DatabaseLib.Common;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayTransaction
	{
		public virtual int Id { get; set; }
		public virtual MP_EbayOrderItem OrderItem { get; set; }
		public virtual DateTime CreatedDate { get; set; }
		public virtual int? QuantityPurchased { get; set; }
		public virtual string PaymentHoldStatus { get; set; }
		public virtual string PaymentMethodUsed { get; set; }
		public virtual AmountInfo TransactionPrice { get; set; }

		public virtual string ItemID { get; set; }
		public virtual string ItemPrivateNotes { get; set; }
		public virtual string ItemSellerInventoryID { get; set; }
		public virtual string ItemSKU { get; set; }
		public virtual string eBayTransactionId { get; set; }
		public virtual MP_EBayOrderItemDetail OrderItemDetail { get; set; }
	}
}