namespace EZBob.DatabaseLib.DatabaseWrapper.Order {
	using System;
	using System.Collections.Generic;
	using EzBob.CommonLib;
	using EzBob.CommonLib.ReceivedDataListLogic;
	using EzBob.CommonLib.TimePeriodLogic;

	[Serializable]
	public class FreeAgentInvoicesList : ReceivedDataListTimeMarketTimeDependentBase<FreeAgentInvoice> {
		public FreeAgentInvoicesList() : base(DateTime.Now, null) {}

		public FreeAgentInvoicesList(DateTime submittedDate, IEnumerable<FreeAgentInvoice> collection = null) 
			: base(submittedDate, collection) {}

		public override ReceivedDataListTimeDependentBase<FreeAgentInvoice> Create(
			DateTime submittedDate,
			IEnumerable<FreeAgentInvoice> collection
		) {
			return new FreeAgentInvoicesList(submittedDate, collection);
		} // Create
	} // class FreeAgentInvoicesList

	[Serializable]
	public class FreeAgentInvoiceItem {
		public string url { get; set; }
		public int position { get; set; }
		public string description { get; set; }
		public string item_type { get; set; }
		public decimal price { get; set; }
		public decimal quantity { get; set; }
		public string category { get; set; }
	} // class FreeAgentInvoiceItem

	[Serializable]
	public class FreeAgentInvoice : TimeDependentRangedDataBase {
		public string url { get; set; }
		public string contact { get; set; }
		public DateTime? dated_on { get; set; }
		public DateTime? due_on { get; set; }
		public string reference { get; set; }
		public string currency { get; set; }
		public decimal exchange_rate { get; set; }
		public decimal net_value { get; set; }
		public decimal total_value { get; set; }
		public decimal paid_value { get; set; }
		public decimal due_value { get; set; }
		public string status { get; set; }
		public bool omit_header { get; set; }
		public int payment_terms_in_days { get; set; }
		public DateTime? paid_on { get; set; }

		public List<FreeAgentInvoiceItem> invoice_items { get; set; }

		public override DateTime RecordTime {
			get { return dated_on.HasValue ? dated_on.Value : new DateTime(1900, 1, 1); }
		} // RecordTime
	} // class FreeAgentInvoice

	[Serializable]
	public class InvoicesListHelper : IFreeAgentItemContainer {
		public List<FreeAgentInvoice> Invoices { get; set; }

		public bool HasItems() {
			return (Invoices != null) && (Invoices.Count > 0);
		} // HasItems

		public int GetItemCount() {
			return HasItems() ? Invoices.Count : 0;
		} // GetItemCount
	} // class InvoicesListHelper
} // namespace
