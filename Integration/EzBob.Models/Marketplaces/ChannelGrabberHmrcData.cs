namespace EzBob.Models.Marketplaces {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;

	#region class ChannelGrabberHmrcData

	[Serializable]
	public class ChannelGrabberHmrcData : IChannelGrabberData {
		public IEnumerable<VatReturnRawData> VatReturn { get; set; }
		public IEnumerable<RtiTaxMonthRawData> RtiTaxMonths { get; set; }
		public BankStatementDataModel BankStatement { get; set; }
		public BankStatementDataModel BankStatementAnnualized { get; set; }
		public decimal SalariesMultiplier { get; set; }
		public VatReturnSummary[] VatReturnSummary { get; set; }
		public VatReturnSummaryDates[] VatReturnSummaryDates { get; set; }
	} // class ChannelGrabberHmrcData

	#endregion class ChannelGrabberHmrcData

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
