namespace EzBob.Models.Marketplaces
{
	using System.Collections.Generic;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;

	public class SageModel
	{
		public IEnumerable<SageSalesInvoice> Invoices { get; set; }
    }
}