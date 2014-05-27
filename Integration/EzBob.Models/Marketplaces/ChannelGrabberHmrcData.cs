namespace EzBob.Models.Marketplaces {
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using Ezbob.Backend.Models;

	#region class ChannelGrabberHmrcData

	[Serializable]
	public class ChannelGrabberHmrcData : IChannelGrabberData {
		public IEnumerable<VatReturnEntry> VatReturn { get; set; }
		public IEnumerable<RtiTaxMonthEntry> RtiTaxMonths { get; set; }
		public BankStatementDataModel BankStatement { get; set; }
		public BankStatementDataModel BankStatementAnnualized { get; set; }
		public decimal SalariesMultiplier { get; set; }
		public VatReturnSummary[] VatReturnSummary { get; set; }
		public VatReturnSummaryDates[] VatReturnSummaryDates { get; set; }
	} // class ChannelGrabberHmrcData

	#endregion class ChannelGrabberHmrcData

	#region class BankStatementDataModel

	[Serializable]
	public class BankStatementDataModel {
		public int PeriodMonthsNum { get; set; }
		public string Period { get; set; }
		public double PercentOfAnnual { get; set; }
		public double Revenues { get; set; }
		public double Opex { get; set; }
		public double TotalValueAdded { get; set; }
		public double PercentOfRevenues { get; set; }
		public double Salaries { get; set; }
		public double Tax { get; set; }
		public double Ebida { get; set; }
		public double PercentOfAnnual2 { get; set; }
		public double ActualLoansRepayment { get; set; }
		public double FreeCashFlow { get; set; }
	} // class BankStatementDataModel

	#endregion class BankStatementDataModel

	#region class VatReturnSummaryDates

	[Serializable]
	public class VatReturnSummaryDates {
		public VatReturnSummaryDates(DateTime oDateFrom, DateTime oDateTo) {
			DateFrom = oDateFrom;
			DateTo = oDateTo;
			TotalSummaryDays = (DateTo - DateFrom).Days;
		} // constructor

		public DateTime DateFrom { get; set; }
		public DateTime DateTo { get; set; }
		public int TotalSummaryDays { get; set; }
	} // class VatReturnSummaryDates

	#endregion class VatReturnSummaryDates
} // namespace EzBob.Models.Marketplaces
