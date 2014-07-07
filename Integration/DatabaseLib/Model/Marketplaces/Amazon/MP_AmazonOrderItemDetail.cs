namespace EZBob.DatabaseLib.Model.Marketplaces.Amazon
{
	using Common;
	using Iesi.Collections.Generic;

	public class MP_AmazonOrderItemDetail
	{
		public MP_AmazonOrderItemDetail()
		{
			OrderItemCategories = new HashedSet<MP_AmazonOrderItemDetailCatgory>();
		}

		public virtual int Id { get; set; }
		public virtual string ASIN { get; set; }
		public virtual AmountInfo CODFee { get; set; }
		public virtual AmountInfo CODFeeDiscount { get; set; }
		public virtual string GiftMessageText { get; set; }
		public virtual string GiftWrapLevel { get; set; }
		public virtual AmountInfo GiftWrapPrice { get; set; }
		public virtual AmountInfo GiftWrapTax { get; set; }
		public virtual AmountInfo ItemPrice { get; set; }
		public virtual AmountInfo ItemTax { get; set; }
		public virtual string OrderItemId { get; set; }
		public virtual AmountInfo PromotionDiscount { get; set; }
		public virtual decimal QuantityOrdered { get; set; }
		public virtual decimal QuantityShipped { get; set; }
		public virtual string SellerSKU { get; set; }
		public virtual AmountInfo ShippingDiscount { get; set; }
		public virtual AmountInfo ShippingPrice { get; set; }
		public virtual AmountInfo ShippingTax { get; set; }
		public virtual string Title { get; set; }

		public virtual MP_AmazonOrderItem OrderItem { get; set; }
		public virtual ISet<MP_AmazonOrderItemDetailCatgory> OrderItemCategories { get; set; }
	}
}