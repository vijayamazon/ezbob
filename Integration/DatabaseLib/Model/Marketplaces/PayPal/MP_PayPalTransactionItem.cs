using System;
using EZBob.DatabaseLib.Common;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_PayPalTransactionItem
	{
		public virtual int Id { get; set; }

		public virtual MP_PayPalTransaction Transaction { get; set; }

		public virtual AmountInfo FeeAmount { get; set; }
		public virtual AmountInfo GrossAmount { get; set; }
		public virtual AmountInfo NetAmount { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual string TimeZone { get; set; }
		public virtual string Type { get; set; }
		public virtual string Status { get; set; }
		public virtual string Payer { get; set; }
		public virtual string PayerDisplayName { get; set; }
		public virtual string PayPalTransactionId { get; set; }
	}
}