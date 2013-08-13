using System;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_PayPalTransactionItem2
	{
		public virtual int Id { get; set; }

		public virtual MP_PayPalTransaction Transaction { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual CurrencyData Currency { get; set; }
		public virtual double FeeAmount { get; set; }
		public virtual double GrossAmount { get; set; }
		public virtual double NetAmount { get; set; }
		public virtual string TimeZone { get; set; }
		public virtual string Type { get; set; }
		public virtual string Status { get; set; }
		public virtual string PayPalTransactionId { get; set; }
	}
}