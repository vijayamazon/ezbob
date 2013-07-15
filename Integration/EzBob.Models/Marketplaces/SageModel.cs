namespace EzBob.Models.Marketplaces
{
	using System.Collections.Generic;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;

	public class SageModel
	{
		public IEnumerable<SageSalesInvoice> SalesInvoices { get; set; }
		public IEnumerable<SageIncome> Incomes { get; set; }
		public IEnumerable<SagePurchaseInvoice> PurchaseInvoices { get; set; }
    }
}