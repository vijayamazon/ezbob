namespace EZBob.DatabaseLib.Repository.Turnover {
	using System;

	public class YodleeAggregation {
		public long YodleeAggregationID {get;set;}		
		public DateTime TheMonth {get;set;}
		public bool IsActive {get;set;}
		public decimal Turnover {get;set;}
		public int NumberOfTransactions {get;set;}
		public decimal TotalExpense {get;set;}
		public decimal TotalIncome {get;set;}
		public decimal NetCashFlow {get;set;}
	} // class YodleeAggregation
} // namespace
