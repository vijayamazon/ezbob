namespace EZBob.DatabaseLib.DatabaseWrapper.Order
{
	using System;
	using System.Collections.Generic;
	using EzBob.CommonLib.ReceivedDataListLogic;
	using EzBob.CommonLib.TimePeriodLogic;

	public class SageIncomesList : ReceivedDataListTimeMarketTimeDependentBase<SageIncome>
	{
		public SageIncomesList()
			: base(DateTime.Now, null)
		{
		}

		public SageIncomesList(DateTime submittedDate, IEnumerable<SageIncome> collection = null)
			: base(submittedDate, collection)
		{
		}

		public override ReceivedDataListTimeDependentBase<SageIncome> Create(DateTime submittedDate,
																			  IEnumerable<SageIncome> collection)
		{
			return new SageIncomesList(submittedDate, collection);
		}
	}

	public class SageIncome : TimeDependentRangedDataBase
	{
		public int SageId { get; set; }
		public DateTime? date { get; set; }
		public DateTime? invoice_date { get; set; }
		public decimal amount { get; set; }
		public decimal tax_amount { get; set; }
		public decimal gross_amount { get; set; }
		public decimal tax_percentage_rate { get; set; }
		public int? tax_code { get; set; }
		public int tax_scheme_period_id { get; set; }
		public string reference { get; set; }
		public int? contact { get; set; }
		public int? source { get; set; }
		public int? destination { get; set; }
		//public int? payment_method { get; set; }
		public bool voided { get; set; }
		public int lock_version { get; set; }

		public override DateTime RecordTime
		{
			get { return date.HasValue ? date.Value : new DateTime(1900, 1, 1); }
		}
	}
}