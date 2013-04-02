using System;
using EZBob.DatabaseLib.Common;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayExternalTransaction
	{
		public virtual int Id { get; set; }
		public virtual MP_EbayOrderItem OrderItem { get; set; }
		public virtual AmountInfo FeeOrCreditAmount { get; set; }
		public virtual AmountInfo PaymentOrRefundAmount { get; set; }
		public virtual string TransactionID { get; set; }
		public virtual DateTime? TransactionTime { get; set; }
	}
}