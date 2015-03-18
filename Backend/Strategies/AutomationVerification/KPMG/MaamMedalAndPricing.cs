namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;

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

			CsvOutput.Add(Datum.CsvTitles(this.loanSources));

			CsvTitles = Datum.CsvTitles(this.loanSources).Split(';');

			foreach (Datum d in Data)
				CsvOutput.Add(d.ToCsv(this.crLoans, this.loanSources));

			Log.Debug(
				"\n\nCSV output - begin:\n{0}\nCSV output - end.\n",
				string.Join("\n", CsvOutput)
			);
		} // Execute

		public virtual List<string> CsvOutput { get; private set; }

		public virtual string[] CsvTitles { get; private set; }

		protected virtual string Condition {
			get { return string.Empty; }
		} // Condition

		private string Query {
			get { return string.Format(QueryFormat, Condition); }
		} // Query

		private void LoadCashRequests() {
			Data.Clear();

			DB.ForEachRowSafe(ProcessRow, Query, CommandSpecies.Text);

			Log.Debug("{0} loaded before filtering.", Grammar.Number(Data.Count, "cash request"));

			var byCustomer = new SortedDictionary<int, List<Datum>>();

			foreach (Datum curDatum in Data) {
				if (!byCustomer.ContainsKey(curDatum.CustomerID)) {
					byCustomer[curDatum.CustomerID] = new List<Datum> { curDatum };
					continue;
				} // if

				List<Datum> customerData = byCustomer[curDatum.CustomerID];

				var lastKnown = customerData.Last(d => !d.IsCampaign && !d.IsSuperseded);

				// EZ-3048: from two cash requests that happen in less than 24 hours only the latest should be taken.
				if ((curDatum.DecisionTime - lastKnown.DecisionTime).TotalHours < 24)
					lastKnown.IsSuperseded = true;

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

		private void ProcessRow(SafeReader sr) {
			Datum d = sr.Fill<Datum>();
			d.Tag = this.tag;

			sr.Fill(d.Manual);
			sr.Fill(d.ManualCfg);

			d.ManualCfg.Calculate(d.Manual);
			d.LoadLoans(DB);

			Data.Add(d);
		} // ProcessRow

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
	r.ScorePoints,
	CONVERT(BIT, CASE WHEN r.UnderwriterComment LIKE '%campaign%' THEN 1 ELSE 0 END) AS IsCampaign
FROM
	CashRequests r
	INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	INNER JOIN CustomerStatuses cs ON c.CollectionStatus = cs.Id
WHERE
	r.IdUnderwriter IS NOT NULL
	AND
	r.IdUnderwriter != 1
	AND
	r.UnderwriterDecision IN ('Approved', 'Rejected', 'ApprovedPending')
	{0}
ORDER BY
	r.IdCustomer ASC,
	r.UnderwriterDecisionDate ASC,
	r.Id ASC";
	} // class MaamMedalAndPricing
} // namespace

