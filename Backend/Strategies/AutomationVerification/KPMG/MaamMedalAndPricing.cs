namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG.Excel;
	using Ezbob.Database;
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

			this.automationTrails = new SortedDictionary<long, AutomationTrails>();

			this.excelGenerator = new ExcelGenerator(Data, this.loanSources, this.automationTrails);
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
				CommandSpecies.StoredProcedure,
				new QueryParameter("Today", spLoad.DateTo ?? new DateTime(2015, 4, 1, 0, 0, 0, DateTimeKind.Utc))
			);

			LoadCashRequests();

			var pc = new ProgressCounter("{0} cash requests processed.", Log, 50);

			foreach (Datum d in Data) {
				bool isHomeOwner = IsHomeOwner(d.CustomerID);

				try {
					d.RunAutomation(isHomeOwner, DB, this.automationTrails);
				} catch (Exception e) {
					Log.Alert(e, "Automation failed for customer {0}.", d.CustomerID);
				} // try

				pc++;
			} // for

			pc.Log();

			this.excelGenerator.Run();
		} // Execute

		public virtual ExcelPackage Xlsx { get { return this.excelGenerator.Xlsx; } }

		protected virtual List<int> RequestedCustomers { get { return this.spLoad.RequestedCustomers; } } // CustomerID

		protected virtual DateTime? DateFrom {
			get { return this.spLoad.DateFrom; }
			set { this.spLoad.DateFrom = value; }
		} // DateFrom

		protected virtual DateTime? DateTo {
			get { return this.spLoad.DateTo; }
			set { this.spLoad.DateTo = value; }
		} // DateTo

		private void LoadCashRequests() {
			Data.Clear();

			var byCustomer = new SortedDictionary<int, CustomerData>();

			ProgressCounter pc = new ProgressCounter("{0} cash requests loaded so far...", Log, 50);

			SetupFeeCalculatorLegacy.ReloadBrokerRepoCache();

			this.spLoad.ForEachResult<SpLoadCashRequestsForAutomationReport.ResultRow>(sr => {
				if (byCustomer.ContainsKey(sr.CustomerID))
					byCustomer[sr.CustomerID].Add(sr);
				else
					byCustomer[sr.CustomerID] = new CustomerData(sr, this.tag, Log);

				pc++;
				return ActionResult.Continue;
			});

			pc.Log();

			Log.Debug("{0} loaded before filtering.", Grammar.Number(Data.Count, "cash request"));

			Data.Clear();

			foreach (CustomerData customerData in byCustomer.Values) {
				customerData.FindLoansAndFilterAraOut(this.crLoans, this.loanSources);
				Data.AddRange(customerData.Data);
			} // for each

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
		private readonly ExcelGenerator excelGenerator;
		private readonly SortedDictionary<long, AutomationTrails> automationTrails;
	} // class MaamMedalAndPricing
} // namespace

