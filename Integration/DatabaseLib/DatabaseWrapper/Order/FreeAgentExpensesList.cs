namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	using System;
	using System.Collections.Generic;
	using EzBob.CommonLib.ReceivedDataListLogic;
	using EzBob.CommonLib.TimePeriodLogic;

	[Serializable]
	public class FreeAgentExpensesList : ReceivedDataListTimeMarketTimeDependentBase<FreeAgentExpense>
	{
		public FreeAgentExpensesList()
			:base (DateTime.Now, null)
		{
		}

		public FreeAgentExpensesList(DateTime submittedDate, IEnumerable<FreeAgentExpense> collection = null) 
			: base(submittedDate, collection)
		{
		}

		public override ReceivedDataListTimeDependentBase<FreeAgentExpense> Create(DateTime submittedDate, IEnumerable<FreeAgentExpense> collection)
		{
			return new FreeAgentExpensesList(submittedDate, collection);
		}
	}

	[Serializable]
	public class FreeAgentExpenseAttachment
	{
		public string url { get; set; }
		public string content_src { get; set; }
		public string content_type { get; set; }
		public string file_name { get; set; }
		public int file_size { get; set; }
		public string description { get; set; }
	}

	[Serializable]
	public class FreeAgentExpense : TimeDependentRangedDataBase
	{
		public string url { get; set; }
		public string user { get; set; }
		public string category { get; set; }
		public DateTime dated_on { get; set; }
		public string currency { get; set; }
		public decimal gross_value { get; set; }
		public decimal native_gross_value { get; set; }
		public decimal? sales_tax_rate { get; set; }
		public decimal sales_tax_value { get; set; }
		public decimal native_sales_tax_value { get; set; }
		public string description { get; set; }
		public decimal? manual_sales_tax_amount { get; set; }
		public DateTime updated_at { get; set; }
		public DateTime created_at { get; set; }

		public FreeAgentExpenseAttachment attachment { get; set; }

		public override DateTime RecordTime
		{
			get { return dated_on; }
		}
	}

	[Serializable]
	public class ExpensesListHelper
	{
		public List<FreeAgentExpense> Expenses { get; set; }
	}
}