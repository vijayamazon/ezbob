namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	using System;
	using System.Collections.Generic;
	using EzBob.CommonLib.ReceivedDataListLogic;
	using EzBob.CommonLib.TimePeriodLogic;

	[Serializable]
	public class FreeAgentOrdersList : ReceivedDataListTimeMarketTimeDependentBase<FreeAgentOrderItem>
	{
		public FreeAgentOrdersList()
			:base (DateTime.Now, null)
		{
		}

		public FreeAgentOrdersList(DateTime submittedDate, IEnumerable<FreeAgentOrderItem> collection = null) 
			: base(submittedDate, collection)
		{
		}

		public override ReceivedDataListTimeDependentBase<FreeAgentOrderItem> Create(DateTime submittedDate, IEnumerable<FreeAgentOrderItem> collection)
		{
			return new FreeAgentOrdersList(submittedDate, collection);
		}
	}

	[Serializable]
	public class FreeAgentOrderItem : TimeDependentRangedDataBase
	{
		public int Id { get; set; }
		public string url { get; set; }
		public string contact { get; set; }
		public DateTime dated_on { get; set; }
		public DateTime due_on { get; set; }
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
		public DateTime paid_on { get; set; }

		public override DateTime RecordTime
		{
			get { return dated_on; }
		}
	}

	[Serializable]
	public class InvoicesList
	{
		public List<FreeAgentOrderItem> Invoices { get; set; }
	}
}