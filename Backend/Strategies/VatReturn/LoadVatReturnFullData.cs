namespace EzBob.Backend.Strategies.VatReturn {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Misc;

	public class LoadVatReturnFullData : AStrategy {
		#region public

		#region constructor

		public LoadVatReturnFullData(int nCustomerID, int nCustomerMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oRaw = new LoadVatReturnRawData(nCustomerMarketplaceID, DB, Log);
			m_oSummary = new LoadVatReturnSummary(nCustomerID, nCustomerMarketplaceID, DB, Log);
			m_oGetBankModel = new GetBankModel(nCustomerID, DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Load VAT return full data"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oRaw.Execute();
			m_oSummary.Execute();

			m_oGetBankModel.Execute();

			BankStatementAnnualized = new BankStatementDataModel();

			if (m_oGetBankModel.Result == null)
				BankStatement = new BankStatementDataModel();
			else {
				BankStatement = m_oGetBankModel.Result.Yodlee.BankStatementDataModel;
				CalculateBankStatements();
			}
		} // Execute

		#endregion method Execute

		public VatReturnRawData[] VatReturnRawData { get { return m_oRaw.VatReturnRawData; } }

		public RtiTaxMonthRawData[] RtiTaxMonthRawData { get { return m_oRaw.RtiTaxMonthRawData; } }

		public VatReturnSummary[] Summary { get { return m_oSummary.Summary; } }

		public BankStatementDataModel BankStatement { get; private set; }

		public BankStatementDataModel BankStatementAnnualized { get; private set; }

		#endregion public

		#region private

		private readonly LoadVatReturnRawData m_oRaw;
		private readonly LoadVatReturnSummary m_oSummary;
		private readonly GetBankModel m_oGetBankModel;

		private void CalculateBankStatements() {
			var lastVatReturn = VatReturnRawData.LastOrDefault();

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

			BankStatement.Revenues = vatRevenues == 0 ? BankStatement.Revenues : BankStatement.Revenues / (double)vatRevenues;
			BankStatement.Opex = Math.Abs(vatOpex == 0 ? BankStatement.Opex : BankStatement.Opex / (double)vatOpex);
			BankStatement.TotalValueAdded = BankStatement.Revenues - BankStatement.Opex;
			BankStatement.PercentOfRevenues = Math.Abs(BankStatement.Revenues - 0) < 0.01 ? 0 : BankStatement.TotalValueAdded / BankStatement.Revenues;
			BankStatement.Ebida = BankStatement.TotalValueAdded + (BankStatement.Salaries + BankStatement.Tax);
			BankStatement.FreeCashFlow = BankStatement.Ebida - BankStatement.ActualLoansRepayment;

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

		#endregion private
	} // class LoadVatReturnFullData
} // namespace EzBob.Backend.Strategies.VatReturn
