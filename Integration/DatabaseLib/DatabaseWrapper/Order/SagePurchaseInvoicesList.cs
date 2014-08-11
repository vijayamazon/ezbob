namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	using System;
	using System.Collections.Generic;
	using EzBob.CommonLib.ReceivedDataListLogic;
	using EzBob.CommonLib.TimePeriodLogic;

	public class SagePurchaseInvoicesList : ReceivedDataListTimeMarketTimeDependentBase<SagePurchaseInvoice>
	{
		public SagePurchaseInvoicesList()
			: base(DateTime.Now, null)
		{
		}

		public SagePurchaseInvoicesList(DateTime submittedDate, IEnumerable<SagePurchaseInvoice> collection = null)
			: base(submittedDate, collection)
		{
		}

		public override ReceivedDataListTimeDependentBase<SagePurchaseInvoice> Create(DateTime submittedDate,
		                                                                      IEnumerable<SagePurchaseInvoice> collection)
		{
			return new SagePurchaseInvoicesList(submittedDate, collection);
		}
	}
	
	public class SagePurchaseInvoiceItem
	{
		public int SageId { get; set; }
		public string description { get; set; }
		public decimal quantity { get; set; }
		public decimal unit_price { get; set; }
		public decimal net_amount { get; set; }
		public decimal tax_amount { get; set; }
		public int? tax_code { get; set; }
		public decimal tax_rate_percentage { get; set; }
		public bool? unit_price_includes_tax { get; set; }
		public int? ledger_account { get; set; }
		public string product_code { get; set; }
		public int? product { get; set; }
		public int? service { get; set; }
		public int lock_version { get; set; }
	}

	public class SagePurchaseInvoice : TimeDependentRangedDataBase
	{
		public int SageId { get; set; }
		public int? status { get; set; }
		public DateTime? due_date { get; set; }
		public DateTime? date { get; set; }
		public string void_reason { get; set; }
		public decimal outstanding_amount { get; set; }
		public decimal total_net_amount { get; set; }
		public decimal total_tax_amount { get; set; }
		public int tax_scheme_period_id { get; set; }
		public int? contact { get; set; }
		public string contact_name { get; set; }
		public string main_address { get; set; }
		public string delivery_address { get; set; }
		public bool? delivery_address_same_as_main { get; set; }
		public string reference { get; set; }
		public string notes { get; set; }
		public string terms_and_conditions { get; set; }
		public int lock_version { get; set; }

		public List<SagePurchaseInvoiceItem> line_items { get; set; }

		public override DateTime RecordTime
		{
			get { return date.HasValue ? date.Value : new DateTime(1900, 1, 1); }
		}
	}
}