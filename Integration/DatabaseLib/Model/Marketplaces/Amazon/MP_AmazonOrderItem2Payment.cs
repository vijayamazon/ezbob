using EZBob.DatabaseLib.Common;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_AmazonOrderItem2Payment
	{
		public virtual int Id { get; set; }

		public virtual AmountInfo MoneyInfo { get; set; }
		public virtual string SubPaymentMethod { get; set; }

		public virtual MP_AmazonOrderItem2 OrderItem { get; set; }
	}
}