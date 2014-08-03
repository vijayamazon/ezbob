namespace EZBob.DatabaseLib.Model.Marketplaces.Sage
{
	using System;

	public class MP_SageIncome
	{
		public virtual int Id { get; set; }

		public virtual MP_SageRequest Request { get; set; }

		public virtual int SageId { get; set; }
		public virtual DateTime? date { get; set; }
		public virtual DateTime? invoice_date { get; set; }
		public virtual decimal amount { get; set; }
		public virtual decimal tax_amount { get; set; }
		public virtual decimal gross_amount { get; set; }
		public virtual decimal tax_percentage_rate { get; set; }
		public virtual int? TaxCodeId { get; set; }
		public virtual int tax_scheme_period_id { get; set; }
		public virtual string reference { get; set; }
		public virtual int? ContactId { get; set; }
		public virtual int? SourceId { get; set; }
		public virtual int? DestinationId { get; set; }
		//public virtual int? PaymentMethodId { get; set; }
		public virtual bool voided { get; set; }
		public virtual int lock_version { get; set; }
	}
}