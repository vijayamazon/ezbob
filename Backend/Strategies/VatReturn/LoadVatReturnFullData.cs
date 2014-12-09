namespace EzBob.Backend.Strategies.VatReturn {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Misc;

	public class LoadVatReturnFullData : AStrategy {

		public LoadVatReturnFullData(int nCustomerID, int nCustomerMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog)
		{
			m_nCustomerID = nCustomerID;
			m_nCustomerMarketplaceID = nCustomerMarketplaceID;
			BankStatement = new BankStatementDataModel();
			BankStatementAnnualized = new BankStatementDataModel();
		} // constructor

		public override string Name {
			get { return "Load VAT return full data"; }
		} // Name

		public override void Execute() {
			m_oRaw = new LoadVatReturnRawData(m_nCustomerMarketplaceID, DB, Log);
			m_oSummary = new LoadVatReturnSummary(m_nCustomerID, m_nCustomerMarketplaceID, DB, Log);
			m_oRaw.Execute();
			m_oSummary.Execute();

		} // Execute

		public VatReturnRawData[] VatReturnRawData { get { return m_oRaw.VatReturnRawData; } }

		public RtiTaxMonthRawData[] RtiTaxMonthRawData { get { return m_oRaw.RtiTaxMonthRawData; } }

		public VatReturnSummary[] Summary { get { return m_oSummary.Summary; } }

		public BankStatementDataModel BankStatement { get; private set; }

		public BankStatementDataModel BankStatementAnnualized { get; private set; }

		private LoadVatReturnRawData m_oRaw;
		private LoadVatReturnSummary m_oSummary;
		private readonly int m_nCustomerID;
		private readonly int m_nCustomerMarketplaceID;

		public void CalculateBankStatements(VatReturnRawData lastVatReturn, BankStatementDataModel bankStatement) {
			decimal box3 = 0;
			decimal box4 = 0;
			decimal box6 = 0;
			decimal box7 = 0;

			if (lastVatReturn != null) {
				foreach (KeyValuePair<string, Coin> vat in lastVatReturn.Data) {
					if (vat.Key.Contains("(Box 3)"))
						box3 = vat.Value.Amount;
					else if (vat.Key.Contains("(Box 4)"))
						box4 = vat.Value.Amount;
					else if (vat.Key.Contains("(Box 6)"))
						box6 = vat.Value.Amount;
					else if (vat.Key.Contains("(Box 7)"))
						box7 = vat.Value.Amount;
				} // foreach
			} // if

			var vatRevenues = 1 + (box6 == 0 ? 0 : (box3 / box6));
			var vatOpex = 1 + (box7 == 0 ? 0 : (box4 / box7));

			BankStatement.Revenues = vatRevenues == 0 ? bankStatement.Revenues : bankStatement.Revenues / (double)vatRevenues;
			BankStatement.Opex = Math.Abs(vatOpex == 0 ? bankStatement.Opex : bankStatement.Opex / (double)vatOpex);
			BankStatement.TotalValueAdded = bankStatement.Revenues - bankStatement.Opex;
			BankStatement.PercentOfRevenues = Math.Abs(bankStatement.Revenues - 0) < 0.01 ? 0 : bankStatement.TotalValueAdded / bankStatement.Revenues;
			BankStatement.Ebida = bankStatement.TotalValueAdded + (bankStatement.Salaries + bankStatement.Tax);
			BankStatement.FreeCashFlow = bankStatement.Ebida - bankStatement.ActualLoansRepayment;

			if (BankStatement.PeriodMonthsNum == 0)
				return;

			const int year = 12;

			BankStatementAnnualized.Revenues = (BankStatement.Revenues / BankStatement.PeriodMonthsNum * year);
			BankStatementAnnualized.Opex = (BankStatement.Opex / BankStatement.PeriodMonthsNum * year);
			BankStatementAnnualized.TotalValueAdded = BankStatementAnnualized.Revenues - BankStatementAnnualized.Opex;
			BankStatementAnnualized.PercentOfRevenues = Math.Abs(BankStatementAnnualized.Revenues) < 0.01 ? 0 : BankStatementAnnualized.TotalValueAdded / BankStatementAnnualized.Revenues;
			BankStatementAnnualized.Salaries = (BankStatement.Salaries / BankStatement.PeriodMonthsNum * year);
			BankStatementAnnualized.Tax = (BankStatement.Tax / BankStatement.PeriodMonthsNum * year);
			BankStatementAnnualized.Ebida = BankStatementAnnualized.TotalValueAdded + (BankStatementAnnualized.Salaries + BankStatementAnnualized.Tax);
			BankStatementAnnualized.ActualLoansRepayment = (BankStatement.ActualLoansRepayment / BankStatement.PeriodMonthsNum * year);
			BankStatementAnnualized.FreeCashFlow = BankStatementAnnualized.Ebida - BankStatementAnnualized.ActualLoansRepayment;
		} // CalculateBankStatements

	} // class LoadVatReturnFullData
} // namespace EzBob.Backend.Strategies.VatReturn
