namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using Ezbob.Database;

	public class LastOfferData {
		public LastOfferData(int customerID) {
			SafeReader lastOfferResults = Library.Instance.DB.GetFirst(
				"GetLastOfferForAutomatedDecision",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerID),
				new QueryParameter("Now", DateTime.UtcNow)
			);

			if (lastOfferResults.IsEmpty)
				return;

			LoanOfferApr = lastOfferResults["APR"];
			LoanOfferRepaymentPeriod = lastOfferResults["RepaymentPeriod"];
			LoanOfferInterestRate = lastOfferResults["InterestRate"];
			ManualSetupFeeAmount = lastOfferResults["ManualSetupFeeAmount"];
			ManualSetupFeePercent = lastOfferResults["ManualSetupFeePercent"];
		} // constructor

		public decimal LoanOfferApr { get; private set; }
		public int LoanOfferRepaymentPeriod { get; private set; }
		public decimal LoanOfferInterestRate { get; private set; }
		public int ManualSetupFeeAmount { get; private set; }
		public decimal ManualSetupFeePercent { get; private set; }
	} // class LastOfferData
} // namespace
