namespace Sage
{
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;

	[Serializable]
	public class SageInvoiceStatus
	{
		public int? key { get; set; }
	}

	[Serializable]
	public class SageInvoiceCarriageTaxCode
	{
		public int? key { get; set; }
	}

	[Serializable]
	public class SageInvoiceContact
	{
		public int? key { get; set; }
	}

	[Serializable]
	public class SageInvoiceItemTaxCode
	{
		public int? key { get; set; }
	}

	[Serializable]
	public class SageInvoiceItemLedgerAccount
	{
		public int? key { get; set; }
	}

	[Serializable]
	public class SageInvoiceItemProduct
	{
		public int? key { get; set; }
	}

	[Serializable]
	public class SageInvoiceItemService
	{
		public int? key { get; set; }
	}

	[Serializable]
	public class SageInvoiceItemSerialization
	{
		public int id { get; set; }
		public string description { get; set; }
		public string quantity { get; set; }
		public string unit_price { get; set; }
		public string net_amount { get; set; }
		public string tax_amount { get; set; }
		public SageInvoiceItemTaxCode tax_code { get; set; }
		public string tax_rate_percentage { get; set; }
		public bool unit_price_includes_tax { get; set; }
		public SageInvoiceItemLedgerAccount ledger_account { get; set; }
		public string product_code { get; set; }
		public SageInvoiceItemProduct product { get; set; }
		public SageInvoiceItemService service { get; set; }
		public int lock_version { get; set; }
	}
	
	[Serializable]
	public class SageInvoiceSerialization
	{
		public int id { get; set; }
		public int SageId { get; set; }
		public string invoice_number { get; set; }
		public SageInvoiceStatus status { get; set; }
		public string due_date { get; set; }
		public string date { get; set; }
		public string void_reason { get; set; }
		public string outstanding_amount { get; set; }
		public string total_net_amount { get; set; }
		public string total_tax_amount { get; set; }
		public int tax_scheme_period_id { get; set; }
		public string carriage { get; set; }
		public SageInvoiceCarriageTaxCode carriage_tax_code { get; set; }
		public string carriage_tax_rate_percentage { get; set; }
		public SageInvoiceContact contact { get; set; }
		public string contact_name { get; set; }
		public string main_address { get; set; }
		public string delivery_address { get; set; }
		public bool delivery_address_same_as_main { get; set; }
		public string reference { get; set; }
		public string notes { get; set; }
		public string terms_and_conditions { get; set; }
		public int lock_version { get; set; }

		public List<SageInvoiceItemSerialization> line_items { get; set; }
	}

	[Serializable]
	public class SageDiagnostic
	{
		public string severity { get; set; }
		public string dataCode { get; set; }
		public string message { get; set; }
		public string source { get; set; }
	}

	[Serializable]
	public class SageInvoicesListHelper
	{
		public List<SageInvoiceSerialization> resources { get; set; }
		public int totalResults { get; set; }
		public int startIndex { get; set; }
		public int itemsPerPage { get; set; }
		public List<SageDiagnostic> diagnoses { get; set; }
	}
}