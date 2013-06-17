namespace EzBob.Models.Marketplaces
{
	using System.Collections.Generic;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;

	public class FreeAgentModel
	{
		public IEnumerable<FreeAgentInvoice> Invoices { get; set; }
		public IEnumerable<FreeAgentExpense> Expenses { get; set; }
    }
}