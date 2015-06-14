namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.DigIntoManualNoSignatureEnoughData {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.ProcessHistory;
	using DbConstants;
	using Ezbob.Database;
	using Ezbob.Utils;

	internal class DeepData {
		static DeepData() {
			AllNonAffirmativeTraces = new SortedSet<string>();
			AllStandardRejectReasons = new SortedSet<string>();
		} // static constructor

		public DeepData(RawSource src) {
			StandardRejectReasons = new SortedSet<string>();
			Marketplaces = new SortedDictionary<int, MarketplaceData>();
			NonAffirmativeTraces = new SortedSet<string>();

			CustomerID = src.CustomerID;
			CashRequestID = src.FirstCashRequestID;
			UnderwriterName = src.UnderwriterName;
			DecisionTime = src.DecisionTime;
			ManualDecision = src.ManualDecision;
			ManualRejectReason = src.UnderwriterComment;
			AutoApproved = src.DecisionStatus;

			Add(src);
		} // constructor

		public void Add(RawSource src) {
			if (src.MpID.HasValue)
				if (!Marketplaces.ContainsKey(src.MpID.Value))
					Marketplaces[src.MpID.Value] = new MarketplaceData(src);

			if (!string.IsNullOrEmpty(src.Reason)) {
				StandardRejectReasons.Add(src.Reason);
				AllStandardRejectReasons.Add(src.Reason);
			} // if

			if (!string.IsNullOrEmpty(src.TraceName)) {
				NonAffirmativeTraces.Add(src.TraceName);
				AllNonAffirmativeTraces.Add(src.TraceName);
			} // if
		} // Add

		public void LoadMonthTurnover(ProgressCounter pc) {
			DB.ForEachRowSafe(
				sr => {
					int mpID = sr["MpID"];
					int distance = sr["Distance"];
					decimal amount = sr["Turnover"];

					if (Marketplaces.ContainsKey(mpID))
						Marketplaces[mpID].MonthTurnover[distance] = amount;

					pc.Increment();
				},
				"GetCustomerTurnoverForAutoDecision",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@IsForApprove", true),
				new QueryParameter("@CustomerID", CustomerID),
				new QueryParameter("@Now", DecisionTime)
			);
		} // LoadMonthTurnover

		public int CustomerID { get; private set; }
		public long CashRequestID { get; private set; }
		public string UnderwriterName { get; private set; }
		public DateTime DecisionTime { get; private set; }

		public DecisionActions ManualDecision { get; private set; }

		public SortedSet<string> StandardRejectReasons { get; private set; }
		public string ManualRejectReason { get; private set; }
		public SortedDictionary<int, MarketplaceData> Marketplaces { get; private set; }

		public DecisionStatus? AutoApproved { get; private set; }

		public SortedSet<string> NonAffirmativeTraces { get; private set; }

		public static SortedSet<string> AllNonAffirmativeTraces { get; private set; }
		public static SortedSet<string> AllStandardRejectReasons { get; private set; }

		public int ConsumerScore { get; set; }

		private AConnection DB { get { return Library.Instance.DB; } }
	} // class DeepData
} // namespace
