namespace Ezbob.Backend.Strategies.AutomationVerification {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using ConfigManager;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
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

			string top = (topCount > 0) ? "TOP " + topCount : string.Empty;

			string condition = (lastCheckedID > 0)
				? "AND r.Id < " + lastCheckedID
				: string.Empty;

			this.data.Clear();

			DB.ForEachRowSafe(
				ProcessRow,
				string.Format(QueryTemplate, top, condition),
				CommandSpecies.Text
			);

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

			var lst = new List<string>();

			Log.Debug("Output data - begin:");

			foreach (Datum d in this.data) {
				Log.Debug("{0}", d);

				lst.Add(d.ToCsv(crLoans, loanSources));
			} // for each

			Log.Debug("Output data - end.");

			Log.Debug(
				"\n\nCSV output - begin:\n{0}\n{1}\nCSV output - end.\n",
				Datum.CsvTitles(loanSources),
				string.Join("\n", lst)
			);
		} // Execute

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
				} else {
					LoanAmount += lmd.LoanAmount;
					RepaidPrincipal += lmd.RepaidPrincipal;

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
					RepaidPrincipal
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
						"{0} loan count;{0} worst loan status;{0} issued amount;{0} repaid amount",
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
			[FieldName("ManualSetupFeePercent")]
			public decimal? Percent { get; set; }

            [FieldName("BrokerSetupFeePercent")]
			public decimal? BrokerPercent { get; set; }

			public void Calculate(AMedalAndPricing map) {
				if (map == null)
					return;

				map.SetupFee = new SetupFeeCalculator(
					Percent,
                    BrokerPercent
				).Calculate(map.Amount);
			} // Calculate

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>
			/// A string that represents the current object.
			/// </returns>
			public override string ToString() {
				return string.Format("( manual pct: {0}, broker pct {1} )",
					Percent.HasValue ? Percent.Value.ToString("P2", CultureInfo.InvariantCulture) : "--",
					BrokerPercent.HasValue ? BrokerPercent.Value.ToString("P2", CultureInfo.InvariantCulture) : "--"
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
					"{0} Decision time;{0} Decision;{0} Amount;{0} Interest Rate;" +
					"{0} Repayment Period;{0} Setup Fee %;{0} Setup Fee Amount",
					prefix
				);
			} // ToCsv

			public string ToCsv() {
				return string.Join(";",
					DecisionTime.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
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

		private const string QueryTemplate = @"
SELECT {0}
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
	r.ManualSetupFeeAmount
FROM
	CashRequests r
	INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	INNER JOIN Security_User u ON r.IdUnderwriter = u.UserId
	INNER JOIN CustomerStatuses cs ON c.CollectionStatus = cs.Id
WHERE
	r.IdUnderwriter IS NOT NULL
	AND
	r.UnderwriterDecision IN ('Approved', 'Rejected', 'ApprovedPending')
	AND
	r.UnderwriterComment NOT LIKE '%campaign%'
	AND
	r.IdUnderwriter IS NOT NULL
	AND
	r.IdUnderwriter != 1
	{1}
ORDER BY
	r.Id DESC";
	} // class MaamMedalAndPricing
} // namespace

