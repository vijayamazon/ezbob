namespace AutomationCalculator.AutoDecision.AutoReRejection {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.ProcessHistory;
	using Newtonsoft.Json;

	public class ReRejectInputData : ITrailInputData {
		public void Init(
			DateTime dataAsOf,
			bool wasManuallyRejected,
			DateTime? lastManualRejectDate,
			bool newDataSourceAdded,
			int openLoansAmount,
			decimal principalRepaymentAmount,
			bool hasLoans,
			decimal autoReRejectMinRepaidPortion,
			int autoReRejectMaxLRDAge
		) {
			DataAsOf = dataAsOf;

			WasManuallyRejected = wasManuallyRejected;
			LastManualRejectDate = lastManualRejectDate;
			NewDataSourceAdded = newDataSourceAdded;
			OpenLoansAmount = openLoansAmount;
			PrincipalRepaymentAmount = principalRepaymentAmount;
			HasLoans = hasLoans;

			//config
			AutoReRejectMaxLRDAge = autoReRejectMaxLRDAge;
			AutoReRejectMinRepaidPortion = autoReRejectMinRepaidPortion;
		} // Init

		public void Init(
			Arguments args,
			Configuration cfg,
			MetaData meta,
			List<Marketplace> oNewMps
		) {
			DataAsOf = args.DataAsOf;

			WasManuallyRejected = meta.LmrID > 0;
			LastManualRejectDate = meta.LmrTime;
			NewDataSourceAdded = oNewMps.Count > 0;
			OpenLoansAmount = meta.TakenLoanAmount;
			PrincipalRepaymentAmount = meta.RepaidPrincipal + meta.SetupFees;
			HasLoans = meta.LoanCount > 0;

			//config
			AutoReRejectMaxLRDAge = cfg.MaxLRDAge;
			AutoReRejectMinRepaidPortion = cfg.MinRepaidPortion;
		} // Init

		public DateTime DataAsOf { get; private set; }

		public bool WasManuallyRejected { get; private set; }
		public DateTime? LastManualRejectDate { get; private set; }
		public bool NewDataSourceAdded { get; private set; }
		public decimal OpenLoansAmount { get; private set; }
		public decimal PrincipalRepaymentAmount { get; private set; }
		public bool HasLoans { get; private set; }

		//config
		public decimal AutoReRejectMinRepaidPortion { get; private set; }
		public int AutoReRejectMaxLRDAge { get; private set; }

		public string Serialize() {
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		} // Serialize
	} // class ReRejectInputData
} // namespace
