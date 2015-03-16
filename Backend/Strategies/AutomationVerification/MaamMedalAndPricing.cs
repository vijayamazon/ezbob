namespace Ezbob.Backend.Strategies.AutomationVerification {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using ConfigManager;
	using EKM.API;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using PaymentServices.Calculators;

	using TCrLoans = System.Collections.Generic.SortedDictionary<
		int,
		System.Collections.Generic.List<MaamMedalAndPricing.LoanMetaData>
	>;

	public class MaamMedalAndPricing : AStrategy {
		public MaamMedalAndPricing(int topCount, int lastCheckedCashRequestID) {
			this.topCount = topCount;
			this.lastCheckedID = lastCheckedCashRequestID;
			this.data = new List<Datum>();
			this.homeOwners = new SortedDictionary<int, bool>();
			this.defaultCustomers = new SortedSet<int>();
			this.crLoans = new TCrLoans();
			this.loanSources = new SortedSet<string>();
		} // constructor

		public override string Name {
			get { return "MaamMedalAndPricing"; }
		} // Name

		public override void Execute() {
			CsvOutput = new List<string>();

			this.loanSources.Clear();
			this.crLoans.Clear();
			this.defaultCustomers.Clear();

			DB.ForEachRowSafe(
				srdc => defaultCustomers.Add(srdc["CustomerID"]),
				"LoadDefaultCustomers",
				CommandSpecies.StoredProcedure
			);

			DB.ForEachResult<LoanMetaData>(
				lmd => {
					if (this.crLoans.ContainsKey(lmd.CashRequestID))
						this.crLoans[lmd.CashRequestID].Add(lmd);
					else
						this.crLoans[lmd.CashRequestID] = new List<LoanMetaData> { lmd };

					this.loanSources.Add(lmd.LoanSourceName);
				},
				"LoadAllLoansMetaData",
				CommandSpecies.StoredProcedure
			);

			LoadCashRequests();

			var pc = new ProgressCounter("{0} cash requests processed.", Log, 50);

			foreach (Datum d in this.data) {
				d.IsDefault = defaultCustomers.Contains(d.CustomerID);
				bool isHomeOwner = IsHomeOwner(d.CustomerID);

				d.CheckAutoReject(DB, Log);

				d.AutoThen.Calculate(d.CustomerID, isHomeOwner, DB, Log);
				d.AutoNow.Calculate(d.CustomerID, isHomeOwner, DB, Log);

				pc++;
			} // for

			pc.Log();

			CsvOutput.Add(Datum.CsvTitles(this.loanSources));

			Log.Debug("Output data - begin:");

			foreach (Datum d in this.data) {
				Log.Debug("{0}", d);

				CsvOutput.Add(d.ToCsv(this.crLoans, this.loanSources));
			} // for each

			Log.Debug("Output data - end.");

			Log.Debug(
				"\n\nCSV output - begin:\n{0}\nCSV output - end.\n",
				string.Join("\n", CsvOutput)
			);
		} // Execute

		public virtual List<string> CsvOutput { get; private set; }

		protected virtual string Condition {
			get { return string.Empty; }
		} // Condition

		private string Query {
			get { return string.Format(QueryFormat, Condition); }
		} // Query

		private void LoadCashRequests() {
			this.data.Clear();

			DB.ForEachRowSafe(ProcessRow, Query, CommandSpecies.Text);

			Log.Debug("{0} loaded before filtering.", Grammar.Number(this.data.Count, "cash request"));

			var byCustomer = new SortedDictionary<int, List<Datum>>();

			foreach (Datum curDatum in this.data) {
				if (!byCustomer.ContainsKey(curDatum.CustomerID)) {
					byCustomer[curDatum.CustomerID] = new List<Datum> { curDatum };
					continue;
				} // if

				List<Datum> customerData = byCustomer[curDatum.CustomerID];

				var lastKnown = customerData.Last();

				// EZ-3048: from two cash requests that happen in less than 24 hours only the latest should be taken.
				if ((curDatum.DecisionTime - lastKnown.DecisionTime).TotalHours < 24)
					customerData.RemoveAt(customerData.Count - 1);

				customerData.Add(curDatum);
			} // for each

			var result = new List<Datum>();

			foreach (List<Datum> lst in byCustomer.Values)
				result.AddRange(lst);

			result.Sort((a, b) => b.CashRequestID.CompareTo(a.CashRequestID));

			if (this.lastCheckedID > 0)
				result = result.Where(ymir => ymir.CashRequestID < this.lastCheckedID).ToList();

			if (this.topCount > 0)
				result = result.Take(this.topCount).ToList();

			this.data.Clear();
			this.data.AddRange(result);

			Log.Debug("{0} remained after filtering.", Grammar.Number(this.data.Count, "cash request"));
		} // LoadCashRequests

		private bool IsHomeOwner(int customerID) {
			if (homeOwners.ContainsKey(customerID))
				return homeOwners[customerID];

			bool isHomeOwnerAccordingToLandRegistry = DB.ExecuteScalar<bool>(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerID)
			);

			homeOwners[customerID] = isHomeOwnerAccordingToLandRegistry;

			return isHomeOwnerAccordingToLandRegistry;
		} // IsHomeOwner

		private void ProcessRow(SafeReader sr) {
			Datum d = sr.Fill<Datum>();
			sr.Fill(d.Manual);
			sr.Fill(d.ManualCfg);

			d.ManualCfg.Calculate(d.Manual);
			d.LoadLoans(DB);

			this.data.Add(d);
		} // ProcessRow

		internal class LoanMetaData : AResultRow {
			public int CashRequestID { get; set; }
			public int LoanID { get; set; }
			public string LoanSourceName { get; set; }
			public DateTime LoanDate { get; set; }
			public decimal LoanAmount { get; set; }
			public string Status {
				get { return LoanStatus.ToString(); }
				set {
					LoanStatus ls;
					Enum.TryParse(value, true, out ls);
					LoanStatus = ls;
				} // set
			} // Status
			public decimal RepaidPrincipal { get; set; }

			public int MaxLateDays { get; set; }

			public LoanStatus LoanStatus { get; protected set; }
		} // class LoanMetaData

		internal class LoanSummaryData : LoanMetaData {
			public LoanSummaryData() {
				Counter = 0;
			} // constructor

			public void Add(LoanMetaData lmd) {
				if (Counter == 0) {
					LoanAmount = lmd.LoanAmount;
					RepaidPrincipal = lmd.RepaidPrincipal;
					LoanStatus = lmd.LoanStatus;
					MaxLateDays = lmd.MaxLateDays;
				} else {
					LoanAmount += lmd.LoanAmount;
					RepaidPrincipal += lmd.RepaidPrincipal;
					MaxLateDays = Math.Max(MaxLateDays, lmd.MaxLateDays);

					if (lmd.LoanStatus == LoanStatus.Late)
						LoanStatus = LoanStatus.Late;
					else if (lmd.LoanStatus == LoanStatus.Live) {
						if (LoanStatus != LoanStatus.Late)
							LoanStatus = LoanStatus.Live;
					} else {
						if ((LoanStatus != LoanStatus.Late) && (LoanStatus != LoanStatus.Live))
							LoanStatus = LoanStatus.PaidOff;
					} // if
				} // if

				Counter++;
			} // Add

			public int Counter { get; private set; }

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>
			/// A string that represents the current object.
			/// </returns>
			public override string ToString() {
				return string.Join(";",
					Counter,
					LoanStatus.ToString(),
					LoanAmount,
					RepaidPrincipal,
					MaxLateDays
				);
			} // ToString
		} // LoanSummaryData

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		private class Datum {
			public Datum() {
				Manual = new ManualMedalAndPricing();
				ManualCfg = new SetupFeeConfiguration();
				AutoThen = new AutoMedalAndPricing();
				AutoNow = new AutoMedalAndPricing { DecisionTime = DateTime.UtcNow, };
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
			public AMedalAndPricing AutoNow { get; private set; }

			public SetupFeeConfiguration ManualCfg { get; private set; }

			public void CheckAutoReject(AConnection db, ASafeLog log) {
				AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent oSecondary =
					new AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent(db, log, CustomerID);

				oSecondary.MakeDecision(oSecondary.GetRejectionInputData(DecisionTime));

				IsAutoRejected = oSecondary.Trail.HasDecided;
			} // CheckAutoReject

			public void LoadLoans(AConnection db) {
				AutoThen.LoadLoans(CustomerID, db);
				AutoNow.LoadLoans(CustomerID, db);
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
					"Is Default Now",
					AMedalAndPricing.CsvTitles("Manual"),
					"Auto reject",
					AMedalAndPricing.CsvTitles("Auto then"),
					AMedalAndPricing.CsvTitles("Auto now"),
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

				foreach (var s in sources)
					os.Add(bySource[s].ToString());

				return string.Join(";",
					CashRequestID.ToString(CultureInfo.InvariantCulture),
					CustomerID.ToString(CultureInfo.InvariantCulture),
					BrokerID.ToString(CultureInfo.InvariantCulture),
					IsDefault,
					Manual.ToCsv(),
					IsAutoRejected ? "Rejected" : "Manual",
					AutoThen.ToCsv(),
					AutoNow.ToCsv(),
					string.Join(";", os)
				);
			} // ToCsv
		} // class Datum

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private class SetupFeeConfiguration {
			public int UseSetupFee { get; set; }
			public bool UseBrokerSetupFee { get; set; }

			[FieldName("ManualSetupFeePercent")]
			public decimal? Percent { get; set; }

			[FieldName("ManualSetupFeeAmount")]
			public decimal? Amount { get; set; }

			public void Calculate(AMedalAndPricing map) {
				if (map == null)
					return;

				map.SetupFee = new SetupFeeCalculator(
					UseSetupFee == 1,
					UseBrokerSetupFee,
					(int)(Amount ?? 0),
					Percent
				).Calculate(map.Amount);
			} // Calculate

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>
			/// A string that represents the current object.
			/// </returns>
			public override string ToString() {
				return string.Format(
					"( use fee: {0}, broker fee: {1}; manual pct: {2}, amount {3} )",
					UseSetupFee,
					UseBrokerSetupFee,
					Percent.HasValue ? Percent.Value.ToString("P2", CultureInfo.InvariantCulture) : "--",
					Amount.HasValue ? Amount.Value.ToString("N2", CultureInfo.InvariantCulture) : "--"
				);
			} // ToString
		} // class SetupFeeConfiguration

		private class ManualMedalAndPricing : AMedalAndPricing {
			protected override decimal SetupFeeAmount {
				get { return SetupFee; }
			} // SetupFeeAmount

			protected override decimal SetupFeePct {
				get { return Amount <= 0 ? 0 : SetupFee / Amount; }
			} // SetupFeePct
		} // ManualMedalAndPricing

		private class AutoMedalAndPricing : AMedalAndPricing {
			protected override decimal SetupFeeAmount {
				get { return SetupFee * Amount; }
			} // SetupFeeAmount

			protected override decimal SetupFeePct {
				get { return SetupFee; }
			} // SetupFeePct
		} // AutoMedalAndPricing

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		private abstract class AMedalAndPricing {
			public int LoanCount { get; private set; }
			[NonTraversable]
			public DateTime DecisionTime { get; set; }
			[NonTraversable]
			public string Decision { get; set; }

			[NonTraversable]
			public string MedalName { get; set; }

			[NonTraversable]
			public decimal? EzbobScore { get; set; }

			public string DecisionStr {
				get {
					switch (Decision) {
					case "approved":
						return "Approved";

					case "ApprovedPending":
						return "Pending";

					case "not approved":
						return "Manual";

					default:
						return Decision;
					} // switch
				} // get
			} // DecisionStr

			public decimal Amount { get; set; }
			public decimal InterestRate { get; set; }
			public decimal SetupFee { get; set; }
			public int RepaymentPeriod { get; set; }

			public void Calculate(int customerID, bool isHomeOwner, AConnection db, ASafeLog log) {
				var instance = new CalculateMedal(customerID, DecisionTime, true, false);
				instance.Execute();

				MedalName = instance.Result.MedalClassification.ToString();

				EzbobScore = instance.Result.TotalScoreNormalized;

				log.Debug("Before capping the offer: {0}", instance.Result);

				int amount = Math.Min(
					instance.Result.RoundOfferedAmount(),
					isHomeOwner
						? CurrentValues.Instance.MaxCapHomeOwner
						: CurrentValues.Instance.MaxCapNotHomeOwner
				);

				var approveAgent = new AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine.SameDataAgent(
					customerID,
					amount,
					(AutomationCalculator.Common.Medal)instance.Result.MedalClassification,
					(AutomationCalculator.Common.MedalType)instance.Result.MedalType,
					(AutomationCalculator.Common.TurnoverType?)instance.Result.TurnoverType,
					DecisionTime,
					db,
					log
				).Init();
				approveAgent.MakeDecision();

				Amount = amount;
				Decision = approveAgent.Trail.GetDecisionName();

				if (amount == 0) {
					RepaymentPeriod = 0;
					InterestRate = 0;
					SetupFee = 0;
				} else {
					var odc = new OfferDualCalculator(
						customerID,
						DecisionTime,
						amount,
						LoanCount > 0,
						instance.Result.MedalClassification
					);

					odc.CalculateOffer();

					RepaymentPeriod = odc.VerifyBoundaries.RepaymentPeriod;
					InterestRate = odc.VerifyBoundaries.InterestRate / 100.0m;
					SetupFee = odc.VerifyBoundaries.SetupFee / 100.0m;
				} // if
			} // Calculate

			public void LoadLoans(int customerID, AConnection db) {
				LoanCount = db.ExecuteScalar<int>(
					"GetCustomerLoanCount",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", customerID),
					new QueryParameter("Now", DecisionTime)
				);
			} // LoadLoans

			public static string CsvTitles(string prefix) {
				return string.Format(
					"{0} Decision time;{0} Medal;{0} Ezbob Score;{0} Decision;{0} Amount;{0} Interest Rate;" +
					"{0} Repayment Period;{0} Setup Fee %;{0} Setup Fee Amount",
					prefix
				);
			} // ToCsv

			public string ToCsv() {
				return string.Join(";",
					DecisionTime.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
					MedalName,
					EzbobScore,
					DecisionStr,
					Amount.ToString(CultureInfo.InvariantCulture),
					InterestRate.ToString(CultureInfo.InvariantCulture),
					RepaymentPeriod.ToString(CultureInfo.InvariantCulture),
					SetupFeePct.ToString(CultureInfo.InvariantCulture),
					SetupFeeAmount.ToString(CultureInfo.InvariantCulture)
				);
			} // ToCsv

			protected abstract decimal SetupFeePct { get; }
			protected abstract decimal SetupFeeAmount { get; }
		} // class MedalAndPricing

		private readonly int topCount;
		private readonly int lastCheckedID;

		private readonly List<Datum> data;
		private readonly SortedDictionary<int, bool> homeOwners;
		private readonly SortedSet<int> defaultCustomers;
		private readonly TCrLoans crLoans;
		private readonly SortedSet<string> loanSources; 

		private const string QueryFormat = @"
SELECT
	r.Id AS CashRequestID,
	r.IdCustomer AS CustomerID,
	c.BrokerID,
	r.UnderwriterDecisionDate,
	r.UnderwriterDecision,
	ISNULL(r.ManagerApprovedSum, 0) AS Amount,
	r.InterestRate,
	ISNULL(r.ApprovedRepaymentPeriod, r.RepaymentPeriod) AS RepaymentPeriod,
	r.UseSetupFee,
	r.UseBrokerSetupFee,
	r.ManualSetupFeePercent,
	r.ManualSetupFeeAmount,
	r.MedalType,
	r.ScorePoints
FROM
	CashRequests r
	INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	INNER JOIN Security_User u ON r.IdUnderwriter = u.UserId
	INNER JOIN CustomerStatuses cs ON c.CollectionStatus = cs.Id
WHERE
	r.IdUnderwriter IS NOT NULL
	AND
	r.IdUnderwriter != 1
	AND
	r.UnderwriterDecision IN ('Approved', 'Rejected', 'ApprovedPending')
	AND
	r.UnderwriterComment NOT LIKE '%campaign%'
	{0}
ORDER BY
	r.IdCustomer ASC,
	r.UnderwriterDecisionDate ASC,
	r.Id ASC";
	} // class MaamMedalAndPricing
} // namespace

