using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper.Order;

namespace EzBob.Models.Marketplaces {
	using System;

	#region class ChannelGrabberHmrcData

	[Serializable]
	public class ChannelGrabberHmrcData : IChannelGrabberData {
		public IEnumerable<VatReturnEntry> VatReturn { get; set; }
		public IEnumerable<RtiTaxMonthEntry> RtiTaxMonths { get; set; }
		public BankStatementDataModel BankStatement { get; set; }
		public BankStatementDataModel BankStatementAnnualized { get; set; }
	} // class ChannelGrabberHmrcData

	#endregion class ChannelGrabberHmrcData

	#region class BankStatementDataModel

	public class BankStatementDataModel
	{
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
	}
	
	#endregion class BankStatementDataModel
} // namespace EzBob.Models.Marketplaces
