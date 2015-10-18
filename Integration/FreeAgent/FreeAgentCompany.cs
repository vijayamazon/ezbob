namespace FreeAgent {
	using System;
	using EzBob.CommonLib;

	[Serializable]
	public class FreeAgentCompany {
		public string url { get; set; }
		public string name { get; set; }
		public string subdomain { get; set; }
		public string type { get; set; }
		public string currency { get; set; }
		public string mileage_units { get; set; }
		public DateTime? company_start_date { get; set; }
		public DateTime? freeagent_start_date { get; set; }
		public DateTime? first_accounting_year_end { get; set; }
		public string company_registration_number { get; set; }
		public string sales_tax_registration_status { get; set; }
		public string sales_tax_registration_number { get; set; }
	} // class FreeAgentCompany

	[Serializable]
	public class FreeAgentCompanyList : IFreeAgentItemContainer {
		public FreeAgentCompany Company { get; set; }

		public bool HasItems() {
			return (Company != null);
		} // HasItems

		public int GetItemCount() {
			return HasItems() ? 1 : 0;
		} // GetItemCount
	} // class FreeAgentCompanyList
} // namespace
