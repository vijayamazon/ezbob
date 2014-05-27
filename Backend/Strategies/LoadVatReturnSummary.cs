namespace EzBob.Backend.Strategies {
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LoadVatReturnSummary : AStrategy {
		#region public

		#region constructor

		public LoadVatReturnSummary(int customerId, int nMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpLoadVatReturnSummary(nMarketplaceID, DB, Log);
			this.customerId = customerId;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Load VAT return summary"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Summary = new VatReturnSummary();

			m_oSp.ForEachRowSafe((sr, bRowsetStart) => {
				if (sr.ContainsField("SummaryID", sNotFoundIndicator: "SummaryPeriodID"))
					sr.Fill(Summary);
				else
					Summary.Quarters.Add(sr.Fill<VatReturnQuarter>());

				return ActionResult.Continue;
			});

			var getExperianAccountsCurrentBalance = new GetExperianAccountsCurrentBalance(customerId, DB, Log);
			getExperianAccountsCurrentBalance.Execute();
			decimal factor = CurrentValues.Instance.FCFFactor;
			decimal newActualLoansRepayment = (getExperianAccountsCurrentBalance.CurrentBalance / factor);
			Summary.ActualLoanRepayment = newActualLoansRepayment;
			Summary.FreeCashFlow -= newActualLoansRepayment;
		} // Execute

		#endregion method Execute

		#region property Summary

		public VatReturnSummary Summary { get; private set; }
		private readonly int customerId;

		#endregion property Summary

		#endregion public

		#region private

		private readonly SpLoadVatReturnSummary m_oSp;

		#region class SpLoadVatReturnSummary

		private class SpLoadVatReturnSummary : AStoredProc {
			public SpLoadVatReturnSummary(int nMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				MarketplaceID = nMarketplaceID;
			} // constructor

			public override bool HasValidParameters() {
				return MarketplaceID > 0;
			} // HasValidParameters

			public int MarketplaceID { get; set; }
		} // class SpLoadVatReturnSummary

		#endregion class SpLoadVatReturnSummary

		#endregion private
	} // class LoadVatReturnSummary
} // namespace EzBob.Backend.Strategies
