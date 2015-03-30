namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;
	using OfficeOpenXml;
	using PaymentServices.Calculators;
	using TCrLoans = System.Collections.Generic.SortedDictionary<
		int,
		System.Collections.Generic.List<LoanMetaData>
	>;

	public class MaamMedalAndPricing : AStrategy {
		public MaamMedalAndPricing() {
			this.spLoad = new SpLoadCashRequestsForAutomationReport(DB, Log);

			Data = new List<Datum>();

			this.homeOwners = new SortedDictionary<int, bool>();
			this.defaultCustomers = new SortedSet<int>();
			this.crLoans = new TCrLoans();
			this.customerLoans = new TCrLoans();
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

					if (this.customerLoans.ContainsKey(lmd.CustomerID))
						this.customerLoans[lmd.CustomerID].Add(lmd);
					else
						this.customerLoans[lmd.CustomerID] = new List<LoanMetaData> { lmd };

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
					d.SetCustomerLoanCount(this.customerLoans);
					d.RunAutomation(isHomeOwner, DB, Log);
				} catch (Exception e) {
					Log.Alert(e, "Automation failed for customer {0} with cash request {1}.", d.CustomerID, d.CashRequestID);
				} // try

				pc++;
			} // for

			pc.Log();

			CsvTitles = Datum.CsvTitles(this.loanSources).Split(';');
			CreateXlsx();
		} // Execute

		public virtual string[] CsvTitles { get; private set; }

		public virtual ExcelPackage Xlsx { get; private set; }

		private void CreateXlsx() {
			Xlsx = new ExcelPackage();

			ExcelWorksheet sheet = Xlsx.CreateSheet("Cash requests", false, CsvTitles);
			ExcelWorksheet statSheet = Xlsx.CreateSheet("Statistics", false);

			int curRow = 2;

			var stats = new List<Stats> {
				new Stats(statSheet, true, true),
				new Stats(statSheet, true, false),
				new Stats(statSheet, false, true),
				new Stats(statSheet, false, false),
			};

			foreach (Datum d in Data) {
				d.ToXlsx(sheet, curRow, CashRequestLoans, LoanSources);
				curRow++;

				foreach (var st in stats)
					st.Add(d);
			} // for each

			Xlsx.AutoFitColumns();

			int row = 1;

			foreach (var st in stats) {
				row = st.ToXlsx(row);
				row++;
			} // for each
		} // CreateXlsx

		protected virtual TCrLoans CashRequestLoans {
			get { return this.crLoans; }
		} // CashRequestLoans

		protected virtual SortedSet<string> LoanSources {
			get { return this.loanSources; }
		} // LoanSources

		protected virtual int? CustomerID {
			get { return this.spLoad.CustomerID; }
			set { this.spLoad.CustomerID = value; }
		} // CustomerID

		protected virtual DateTime? DateFrom {
			get { return this.spLoad.DateFrom; }
			set { this.spLoad.DateFrom = value; }
		} // DateFrom

		private void LoadCashRequests() {
			Data.Clear();

			var byCustomer = new SortedDictionary<int, CustomerData>();

			ProgressCounter pc = new ProgressCounter("{0} cash requests loaded so far...", Log, 50);

			SetupFeeCalculator.ReloadBrokerRepoCache();

			this.spLoad.ForEachResult<SpLoadCashRequestsForAutomationReport.ResultRow>(sr => {

				if (byCustomer.ContainsKey(sr.CustomerID))
					byCustomer[sr.CustomerID].Add(sr);
				else
					byCustomer[sr.CustomerID] = new CustomerData(sr);

				pc++;
				return ActionResult.Continue;
			});

			pc.Log();

			Log.Debug("{0} loaded before filtering.", Grammar.Number(Data.Count, "cash request"));

			Data.Clear();

			foreach (var customerData in byCustomer.Values)
				Data.AddRange(customerData.Data);

			Log.Debug("{0} remained after filtering cash requests.", Grammar.Number(Data.Count, "data item"));
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
		} // ProcessCashRequest

		private readonly string tag;

		private readonly SortedDictionary<int, bool> homeOwners;
		private readonly SortedSet<int> defaultCustomers;
		private readonly TCrLoans crLoans;
		private readonly TCrLoans customerLoans;
		private readonly SortedSet<string> loanSources; 
		private readonly SpLoadCashRequestsForAutomationReport spLoad;
	} // class MaamMedalAndPricing
} // namespace

