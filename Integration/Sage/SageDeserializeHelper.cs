namespace Sage
{
	using System;
	using System.Collections.Generic;
	
	[Serializable]
	public class SageReferenceKey
	{
		public int? key { get; set; }
	}

	[Serializable]
	public class SageInvoiceItemDeserialization
	{
		public int id { get; set; }
		public string description { get; set; }
		public string quantity { get; set; }
		public string unit_price { get; set; }
		public string net_amount { get; set; }
		public string tax_amount { get; set; }
		public SageReferenceKey tax_code { get; set; }
		public string tax_rate_percentage { get; set; }
		public bool? unit_price_includes_tax { get; set; }
		public SageReferenceKey ledger_account { get; set; }
		public string product_code { get; set; }
		public SageReferenceKey product { get; set; }
		public int lock_version { get; set; }
	}
	
	[Serializable]
	public class SageSalesInvoiceDeserialization
	{
		public int id { get; set; }
		public string invoice_number { get; set; }
		public SageReferenceKey status { get; set; }
		public string due_date { get; set; }
		public string date { get; set; }
		public string void_reason { get; set; }
		public string outstanding_amount { get; set; }
		public string total_net_amount { get; set; }
		public string total_tax_amount { get; set; }
		public string tax_scheme_period_id { get; set; }
		public string carriage { get; set; }
		public SageReferenceKey carriage_tax_code { get; set; }
		public string carriage_tax_rate_percentage { get; set; }
		public SageReferenceKey contact { get; set; }
		public string contact_name { get; set; }
		public string main_address { get; set; }
		public string delivery_address { get; set; }
		public bool? delivery_address_same_as_main { get; set; }
		public string reference { get; set; }
		public string notes { get; set; }
		public string terms_and_conditions { get; set; }
		public int lock_version { get; set; }

		public List<SageInvoiceItemDeserialization> line_items { get; set; }
	}
	
	[Serializable]
	public class SagePaymentStatusDeserialization
	{
		public int id { get; set; }
		public string name { get; set; }
	}

	[Serializable]
	public class SagePurchaseInvoiceDeserialization
	{
		public int id { get; set; }
		public SageReferenceKey status { get; set; }
		public string due_date { get; set; }
		public string date { get; set; }
		public string void_reason { get; set; }
		public string outstanding_amount { get; set; }
		public string total_net_amount { get; set; }
		public string total_tax_amount { get; set; }
		public string tax_scheme_period_id { get; set; }
		public SageReferenceKey contact { get; set; }
		public string contact_name { get; set; }
		public string main_address { get; set; }
		public string delivery_address { get; set; }
		public bool? delivery_address_same_as_main { get; set; }
		public string reference { get; set; }
		public string notes { get; set; }
		public string terms_and_conditions { get; set; }
		public int lock_version { get; set; }

		public List<SageInvoiceItemDeserialization> line_items { get; set; }
	}


	[Serializable]
	public class SageIncomeDeserialization
	{
		public int id { get; set; }
		public string date { get; set; }
		public string invoice_date { get; set; }
		public string amount { get; set; }
		public string tax_amount { get; set; }
		public string gross_amount { get; set; }
		public string tax_percentage_rate { get; set; }
		public SageReferenceKey tax_code { get; set; }
		public int tax_scheme_period_id { get; set; }
		public string reference { get; set; }
		public SageReferenceKey contact { get; set; }
		public SageReferenceKey source { get; set; }
		public SageReferenceKey destination { get; set; }
		//public SageReferenceKey payment_method { get; set; }
		public bool? voided { get; set; }
		public int lock_version { get; set; }
	}

	[Serializable]
	public class SageExpenditureDeserialization
	{
		public int id { get; set; }
		public string date { get; set; }
		public string invoice_date { get; set; }
		public string amount { get; set; }
		public string tax_amount { get; set; }
		public string gross_amount { get; set; }
		public string tax_percentage_rate { get; set; }
		public SageReferenceKey tax_code { get; set; }
		public int tax_scheme_period_id { get; set; }
		public string reference { get; set; }
		public SageReferenceKey contact { get; set; }
		public SageReferenceKey source { get; set; }
		public SageReferenceKey destination { get; set; }
		//public SageReferenceKey payment_method { get; set; }
		public bool? voided { get; set; }
		public int lock_version { get; set; }
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
	public class PaginatedResults<TDeserializationObject>
	{
		public int totalResults { get; set; }
		public int startIndex { get; set; }
		public int itemsPerPage { get; set; }
		public List<SageDiagnostic> diagnoses { get; set; }
		public List<TDeserializationObject> resources { get; set; }
	}
}