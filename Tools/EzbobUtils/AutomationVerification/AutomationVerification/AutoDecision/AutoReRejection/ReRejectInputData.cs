namespace AutomationCalculator.AutoDecision.AutoReRejection {
	using System;
	using System.Collections.Generic;
	using ProcessHistory;
	using Newtonsoft.Json;

	public class ReRejectInputData : ITrailInputData {
		public void Init(
			DateTime dataAsOf,
			bool lastDecisionWasReject,
			DateTime? lastRejectDate,
			DateTime? lastDecisionDate,
			bool newDataSourceAdded,
			int openLoansAmount,
			decimal principalRepaymentAmount,
			int numOfOpenLoans,
			decimal autoReRejectMinRepaidPortion,
			int autoReRejectMaxLRDAge,
			int autoReRejectMaxAllowedLoans
		) {
			DataAsOf = dataAsOf;

			LastDecisionWasReject = lastDecisionWasReject;
			LastRejectDate = lastRejectDate;
			LastDecisionDate = lastDecisionDate;
			NewDataSourceAdded = newDataSourceAdded;
			OpenLoansAmount = openLoansAmount;
			PrincipalRepaymentAmount = principalRepaymentAmount;
			NumOfOpenLoans = numOfOpenLoans;

			//config
			AutoReRejectMaxLRDAge = autoReRejectMaxLRDAge;
			AutoReRejectMinRepaidPortion = autoReRejectMinRepaidPortion;
			AutoReRejectMaxAllowedLoans = autoReRejectMaxAllowedLoans;
		} // Init

		public void Init(
			Arguments args,
			Configuration cfg,
			MetaData meta,
			List<Marketplace> oNewMps
		) {
			DataAsOf = args.DataAsOf;

			LastDecisionWasReject =meta.LastDecisionWasReject;
			LastRejectDate = meta.LastRejectDate;
			LastDecisionDate = meta.LastDecisionDate;
			NumOfOpenLoans = meta.NumOfOpenLoans;

			NewDataSourceAdded = oNewMps.Count > 0;
			OpenLoansAmount = meta.TakenLoanAmount;
			PrincipalRepaymentAmount = meta.RepaidPrincipal;

			//config
			AutoReRejectMaxLRDAge = cfg.MaxLRDAge;
			AutoReRejectMinRepaidPortion = cfg.MinRepaidPortion;
			AutoReRejectMaxAllowedLoans = cfg.MaxAllowedLoans;
		} // Init

		public DateTime DataAsOf { get; private set; }

		public bool LastDecisionWasReject { get; private set; }
		public DateTime? LastRejectDate { get; private set; }
		public DateTime? LastDecisionDate { get; private set; }
		public bool NewDataSourceAdded { get; private set; }
		public decimal OpenLoansAmount { get; private set; }
		public decimal PrincipalRepaymentAmount { get; private set; }
		public int NumOfOpenLoans { get; private set; }

		//config
		public decimal AutoReRejectMinRepaidPortion { get; private set; }
		public int AutoReRejectMaxAllowedLoans { get; private set; }
		public int AutoReRejectMaxLRDAge { get; private set; }

		public string Serialize()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		} // Serialize
	} // class ReRejectInputData
} // namespace
