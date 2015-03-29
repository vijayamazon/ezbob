namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;
	using PaymentServices.Calculators;
	using TCrLoans = System.Collections.Generic.SortedDictionary<
		int,
		System.Collections.Generic.List<LoanMetaData>
	>;

	public class MaamMedalAndPricing : AStrategy {
		public MaamMedalAndPricing(int topCount, int lastCheckedCashRequestID) {
			this.topCount = topCount;
			this.lastCheckedID = lastCheckedCashRequestID;
			Data = new List<Datum>();
			this.homeOwners = new SortedDictionary<int, bool>();
			this.defaultCustomers = new SortedSet<int>();
			this.crLoans = new TCrLoans();
			this.loanSources = new SortedSet<string>();

			this.tag = string.Format(
				"#MaamMedalAndPricing_{0}_{1}",
				DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture),
				Guid.NewGuid().ToString("N")
			);
		} // constructor

		public override string Name {
			get { return "MaamMedalAndPricing"; }
		} // Name

		public List<Datum> Data { get; private set; }

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

			LoadCashRequests();

			var pc = new ProgressCounter("{0} cash requests processed.", Log, 50);

			foreach (Datum d in Data) {
				d.IsDefault = defaultCustomers.Contains(d.CustomerID);
				bool isHomeOwner = IsHomeOwner(d.CustomerID);

				try {
					d.RunAutomation(isHomeOwner, DB, Log);
				} catch (Exception e) {
					Log.Alert(e, "Automation failed for customer {0} with cash request {1}.", d.CustomerID, d.CashRequestID);
				} // try

				pc++;
			} // for

			pc.Log();

			CsvTitles = Datum.CsvTitles(this.loanSources).Split(';');
		} // Execute

		public virtual string[] CsvTitles { get; private set; }

		protected virtual TCrLoans CashRequestLoans {
			get { return this.crLoans; }
		} // CashRequestLoans

		protected virtual SortedSet<string> LoanSources {
			get { return this.loanSources; }
		} // LoanSources

		protected virtual string Condition {
			get { return string.Empty; }
		} // Condition

		private string Query {
			get { return string.Format(QueryFormat, Condition); }
		} // Query

		private void LoadCashRequests() {
			Data.Clear();

			this.crPc = new ProgressCounter("{0} cash requests loaded so far...", Log, 50);

			SetupFeeCalculator.ReloadBrokerRepoCache();

			DB.ForEachRowSafe(ProcessCashRequest, Query, CommandSpecies.Text);

			this.crPc.Log();

			Log.Debug("{0} loaded before filtering.", Grammar.Number(Data.Count, "cash request"));

			var byCustomer = new SortedDictionary<int, List<Datum>>();

			foreach (Datum curDatum in Data) {
				if (!byCustomer.ContainsKey(curDatum.CustomerID)) {
					byCustomer[curDatum.CustomerID] = new List<Datum> { curDatum };
					continue;
				} // if

				List<Datum> customerData = byCustomer[curDatum.CustomerID];

				if (!curDatum.IsCampaign) {
					var lastKnown = customerData.LastOrDefault(d => !d.IsCampaign && !d.IsSuperseded);

					// EZ-3048: from two cash requests that happen in less than 24 hours only the latest should be taken.
					if (lastKnown != null)
						if ((curDatum.DecisionTime - lastKnown.DecisionTime).TotalHours < 24)
							lastKnown.IsSuperseded = true;
				} // if

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

			Data.Clear();
			Data.AddRange(result);

			Log.Debug("{0} remained after filtering.", Grammar.Number(Data.Count, "cash request"));
		} // LoadCashRequests

		private bool IsHomeOwner(int customerID) {
			if (this.homeOwners.ContainsKey(customerID))
				return this.homeOwners[customerID];

			bool isHomeOwnerAccordingToLandRegistry = DB.ExecuteScalar<bool>(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerID)
			);

			this.homeOwners[customerID] = isHomeOwnerAccordingToLandRegistry;

			return isHomeOwnerAccordingToLandRegistry;
		} // IsHomeOwner

		private void ProcessCashRequest(SafeReader sr) {
			Datum d = sr.Fill<Datum>();
			d.Tag = this.tag;

			sr.Fill(d.Manual);
			sr.Fill(d.ManualCfg);

			d.ManualCfg.Calculate(d.Manual);

			Data.Add(d);

			this.crPc++;
		} // ProcessCashRequest

		private ProgressCounter crPc;

		private readonly string tag;
		private readonly int topCount;
		private readonly int lastCheckedID;

		private readonly SortedDictionary<int, bool> homeOwners;
		private readonly SortedSet<int> defaultCustomers;
		private readonly TCrLoans crLoans;
		private readonly SortedSet<string> loanSources; 

		private const string QueryFormat = @"
SELECT
	r.Id AS CashRequestID,
	r.IdCustomer AS CustomerID,
	c.BrokerID,
	CASE
		WHEN r.IdUnderwriter IS NULL THEN r.SystemDecisionDate
		ELSE r.UnderwriterDecisionDate
	END AS UnderwriterDecisionDate,
	CASE
		WHEN (r.IdUnderwriter IS NOT NULL AND r.UnderwriterDecision = 'Rejected') THEN 'Rejected'
		WHEN (r.IdUnderwriter IS NOT NULL AND r.UnderwriterDecision IN ('Approved', 'ApprovedPending')) THEN 'Approved'
		WHEN (r.IdUnderwriter IS NULL AND r.SystemDecision = 'Approve') THEN 'Approved'
	END AS UnderwriterDecision,
	ISNULL(CASE
		WHEN r.IdUnderwriter IS NULL
			THEN CASE
				WHEN r.UnderwriterComment = 'Auto Re-Approval' THEN r.ManagerApprovedSum
				ELSE r.SystemCalculatedSum
			END
		ELSE
			ISNULL(r.ManagerApprovedSum, r.SystemCalculatedSum)
	END, 0) AS Amount,
	r.InterestRate,
	ISNULL(r.ApprovedRepaymentPeriod, r.RepaymentPeriod) AS RepaymentPeriod,
	r.UseSetupFee,
	r.UseBrokerSetupFee,
	r.ManualSetupFeePercent,
	r.ManualSetupFeeAmount,
	r.MedalType,
	r.ScorePoints,
	CONVERT(BIT, CASE WHEN r.UnderwriterComment LIKE '%campaign%' THEN 1 ELSE 0 END) AS IsCampaign,
	ISNULL((
		SELECT COUNT(*)
		FROM Loan
		WHERE CustomerID = r.IdCustomer
		AND [Date] < r.UnderwriterDecisionDate
	), 0) AS LoanCount
FROM
	CashRequests r
	INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	INNER JOIN CustomerStatuses cs ON c.CollectionStatus = cs.Id
WHERE
	r.CreationDate >= 'Sep 4 2012'
	AND
	(r.IdUnderwriter IS NULL OR r.IdUnderwriter != 1)
	AND (
		(r.IdUnderwriter IS NOT NULL AND r.UnderwriterDecision = 'Rejected')
		OR (
			(r.IdUnderwriter IS NOT NULL AND r.UnderwriterDecision IN ('Approved', 'ApprovedPending'))
			OR
			(r.IdUnderwriter IS NULL AND r.SystemDecision = 'Approve')
		)
	)
	{0}
ORDER BY
	r.IdCustomer ASC,
	r.UnderwriterDecisionDate ASC,
	r.Id ASC";
	} // class MaamMedalAndPricing
} // namespace

