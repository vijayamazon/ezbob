namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database.Loans;

	using TCrLoans = System.Collections.Generic.SortedDictionary<
		int,
		System.Collections.Generic.List<LoanMetaData>
	>;

	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	internal class Datum {
		public Datum() {
			Manual = new ManualMedalAndPricing();
			ManualCfg = new SetupFeeConfiguration();
			AutoThen = new AutoMedalAndPricing();
		} // constructor

		public int CashRequestID { get; set; }
		public int CustomerID { get; set; }
		public int BrokerID { get; set; }

		public string MedalType {
			get { return Manual.MedalName; }
			set { Manual.MedalName = value; }
		} // MedalType

		public decimal? ScorePoints {
			get { return Manual.EzbobScore; }
			set { Manual.EzbobScore = value; }
		} // ScorePoints

		[FieldName("UnderwriterDecisionDate")]
		public DateTime DecisionTime {
			get { return Manual.DecisionTime; }
			set {
				Manual.DecisionTime = value;
				AutoThen.DecisionTime = value;
			} // set
		} // DecisionTime

		public DateTime Now { get; set; }

		[FieldName("UnderwriterDecision")]
		public string Decision {
			get { return Manual.Decision; }
			set { Manual.Decision = value; }
		} // Decision

		public AMedalAndPricing Manual { get; private set; }

		public bool IsDefault { get; set; }

		public bool IsAutoRejected { get; private set; }

		public AMedalAndPricing AutoThen { get; private set; }

		public SetupFeeConfiguration ManualCfg { get; private set; }

		public void CheckAutoReject(AConnection db, ASafeLog log) {
			AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent oSecondary =
				new AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent(db, log, CustomerID);

			oSecondary.MakeDecision(oSecondary.GetRejectionInputData(DecisionTime));

			IsAutoRejected = oSecondary.Trail.HasDecided;
		} // CheckAutoReject

		public void LoadLoans(AConnection db) {
			AutoThen.LoadLoans(CustomerID, db);
		} // LoadLoans

		public static string CsvTitles(SortedSet<string> sources) {
			var os = new List<string>();

			foreach (var s in sources) {
				os.Add(string.Format(
					"{0} loan count;{0} worst loan status;{0} issued amount;{0} repaid amount;{0} max late days",
					s
				));
			} // for each

			return string.Join(";",
				"Cash Request ID",
				"Customer ID",
				"Broker ID",
				"Customer is default now",
				"Has default loan",
				AMedalAndPricing.CsvTitles("Manual"),
				"Auto reject",
				AMedalAndPricing.CsvTitles("Auto"),
				string.Join(";", os)
			);
		} // CsvTitles

		public string ToCsv(TCrLoans crLoans, SortedSet<string> sources) {
			List<LoanMetaData> lst = crLoans.ContainsKey(CashRequestID)
				? crLoans[CashRequestID]
				: new List<LoanMetaData>();

			var bySource = new SortedDictionary<string, LoanSummaryData>();

			foreach (var s in sources)
				bySource[s] = new LoanSummaryData();

			foreach (var lmd in lst)
				bySource[lmd.LoanSourceName].Add(lmd);

			var os = new List<string>();

			bool hasDefaultLoan = false;

			foreach (string s in sources) {
				LoanSummaryData loanStat = bySource[s];

				if (loanStat.LoanStatus == LoanStatus.Late)
					hasDefaultLoan = true;

				os.Add(loanStat.ToString());
			} // for each

			return string.Join(";",
				CashRequestID.ToString(CultureInfo.InvariantCulture),
				CustomerID.ToString(CultureInfo.InvariantCulture),
				BrokerID.ToString(CultureInfo.InvariantCulture),
				IsDefault ? "Default" : "No",
				hasDefaultLoan ? "Default" : "No",
				Manual.ToCsv(),
				IsAutoRejected ? "Rejected" : "Manual",
				AutoThen.ToCsv(),
				string.Join(";", os)
			);
		} // ToCsv
	} // class Datum
} // namespace
