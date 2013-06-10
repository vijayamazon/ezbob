namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent
{
	public class MP_FreeAgentInvoiceItem
	{
		public virtual int Id { get; set; }

		public virtual MP_FreeAgentInvoice Invoice { get; set; }

		public virtual string url { get; set; }
		public virtual int position { get; set; }
		public virtual string description { get; set; }
		public virtual string item_type { get; set; }
		public virtual decimal price { get; set; }
		public virtual decimal quantity { get; set; }
		public virtual string category { get; set; }
	}
}