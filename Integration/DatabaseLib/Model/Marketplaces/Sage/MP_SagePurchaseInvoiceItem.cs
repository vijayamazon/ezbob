namespace EZBob.DatabaseLib.Model.Marketplaces.Sage
{
	public class MP_SagePurchaseInvoiceItem
	{
		public virtual int Id { get; set; }

		public virtual MP_SagePurchaseInvoice PurchaseInvoice { get; set; }
		
		public virtual int SageId { get; set; }
		public virtual string description { get; set; }
		public virtual decimal quantity { get; set; }
		public virtual decimal unit_price { get; set; }
		public virtual decimal net_amount { get; set; }
		public virtual decimal tax_amount { get; set; }
		public virtual int? TaxCodeId { get; set; }
		public virtual decimal tax_rate_percentage { get; set; }
		public virtual bool? unit_price_includes_tax { get; set; }
		public virtual int? LedgerAccountId { get; set; }
		public virtual string product_code { get; set; }
		public virtual int? ProductId { get; set; }
		public virtual int? ServiceId { get; set; }
		public virtual int lock_version { get; set; }
	}
}