namespace EZBob.DatabaseLib.Model.Marketplaces.Sage
{
	using System;
	using Iesi.Collections.Generic;

	public class MP_SagePurchaseInvoice
	{
		public virtual int Id { get; set; }

		public virtual MP_SageRequest Request { get; set; }

		public virtual ISet<MP_SagePurchaseInvoiceItem> Items { get; set; }
		
		public virtual int SageId { get; set; }
		public virtual int? StatusId { get; set; }
		public virtual DateTime? due_date { get; set; }
		public virtual DateTime? date { get; set; }
		public virtual string void_reason { get; set; }
		public virtual decimal outstanding_amount { get; set; }
		public virtual decimal total_net_amount { get; set; }
		public virtual decimal total_tax_amount { get; set; }
		public virtual int tax_scheme_period_id { get; set; }
		public virtual int? ContactId { get; set; }
		public virtual string contact_name { get; set; }
		public virtual string main_address { get; set; }
		public virtual string delivery_address { get; set; }
		public virtual bool? delivery_address_same_as_main { get; set; }
		public virtual string reference { get; set; }
		public virtual string notes { get; set; }
		public virtual string terms_and_conditions { get; set; }
		public virtual int lock_version { get; set; }
	}
}