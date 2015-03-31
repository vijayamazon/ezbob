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
		long,
		System.Collections.Generic.List<LoanMetaData>
	>;

	public class MaamMedalAndPricing : AStrategy {
		public MaamMedalAndPricing() {
			this.spLoad = new SpLoadCashRequestsForAutomationReport(DB, Log);

			Data = new List<Datum>();

			this.homeOwners = new SortedDictionary<int, bool>();
			this.crLoans = new TCrLoans();
			this.loanSources = new SortedSet<string>();

			this.tag = string.Format(
				"#MaamMedalAndPricing_{0}_{1}",
				DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture),
				Guid.NewGuid().ToString("N")
			);

			Log.Debug("The tag is '{0}'.", this.tag);
		} // constructor

		public override string Name {
			get { return "MaamMedalAndPricing"; }
		} // Name

		public List<Datum> Data { get; private set; }

		public override void Execute() {
			this.loanSources.Clear();
			this.crLoans.Clear();

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
				bool isHomeOwner = IsHomeOwner(d.CustomerID);

				try {
					d.FindLoans(CashRequestLoans, LoanSources);
					d.AutoFirst.RunAutomation(isHomeOwner, DB, Log);
					d.AutoLast.RunAutomation(isHomeOwner, DB, Log);
				} catch (Exception e) {
					Log.Alert(e, "Automation failed for customer {0} at {1}.", d.CustomerID, d.FirstManual.DecisionTime);
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
				new Stats(statSheet, true, false),
				new Stats(statSheet, true, true),
			};

			foreach (Datum d in Data) {
				d.ToXlsx(sheet, curRow);
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
					byCustomer[sr.CustomerID] = new CustomerData(sr, this.tag);

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

		private readonly string tag;

		private readonly SortedDictionary<int, bool> homeOwners;
		private readonly TCrLoans crLoans;
		private readonly SortedSet<string> loanSources; 
		private readonly SpLoadCashRequestsForAutomationReport spLoad;
	} // class MaamMedalAndPricing
} // namespace

