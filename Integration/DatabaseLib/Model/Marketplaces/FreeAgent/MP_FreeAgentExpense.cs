namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent
{
	using System;

	public class MP_FreeAgentExpense
	{
		public virtual int Id { get; set; }

		public virtual MP_FreeAgentRequest Request { get; set; }

		public virtual string url { get; set; }
		public virtual string username { get; set; }
		public virtual string category { get; set; }
		public virtual DateTime dated_on { get; set; }
		public virtual string currency { get; set; }
		public virtual decimal gross_value { get; set; }
		public virtual decimal native_gross_value { get; set; }
		public virtual decimal? sales_tax_rate { get; set; }
		public virtual decimal sales_tax_value { get; set; }
		public virtual decimal native_sales_tax_value { get; set; }
		public virtual string description { get; set; }
		public virtual decimal? manual_sales_tax_amount { get; set; }
		public virtual DateTime updated_at { get; set; }
		public virtual DateTime created_at { get; set; }

		public virtual string attachment_url { get; set; }
		public virtual string attachment_content_src { get; set; }
		public virtual string attachment_content_type { get; set; }
		public virtual string attachment_file_name { get; set; }
		public virtual int attachment_file_size { get; set; }
		public virtual string attachment_description { get; set; }
	}
}