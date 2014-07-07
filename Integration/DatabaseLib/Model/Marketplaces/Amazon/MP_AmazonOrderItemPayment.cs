namespace EZBob.DatabaseLib.Model.Marketplaces.Amazon
{
	using Common;

	public class MP_AmazonOrderItemPayment
	{
		public virtual int Id { get; set; }

		public virtual AmountInfo MoneyInfo { get; set; }
		public virtual string SubPaymentMethod { get; set; }

		public virtual MP_AmazonOrderItem OrderItem { get; set; }
	}
}