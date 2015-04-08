namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Collections.Generic;
	using System.Drawing;
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
				CommandSpecies.StoredProcedure,
				new QueryParameter("Today", spLoad.DateTo ?? new DateTime(2015, 4, 1, 0, 0, 0, DateTimeKind.Utc))
			);

			LoadCashRequests();

			var pc = new ProgressCounter("{0} cash requests processed.", Log, 50);

			foreach (Datum d in Data) {
				bool isHomeOwner = IsHomeOwner(d.CustomerID);

				try {
					d.RunAutomation(isHomeOwner, DB);
				} catch (Exception e) {
					Log.Alert(e, "Automation failed for customer {0}.", d.CustomerID);
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

			ExcelWorksheet statSheet = Xlsx.CreateSheet("Statistics", false);
			ExcelWorksheet decisionSheet = Xlsx.CreateSheet("Decisions", false, CsvTitles);
			ExcelWorksheet loanIDSheet = Xlsx.CreateSheet("Loan IDs", false);

			var decisionStats = new Stats(Log, statSheet, true, "at last decision time");

			var stats = new List<Tuple<Stats, int>> {
				new Tuple<Stats, int>(decisionStats, -1),
			};

			var pc = new ProgressCounter("{0} items sent to .xlsx", Log, 50);

			int curRow = 2;

			foreach (Datum d in Data) {
				d.ToXlsx(decisionSheet, curRow);
				curRow++;

				foreach (Tuple<Stats, int> pair in stats)
					pair.Item1.Add(d, pair.Item2);

				pc++;
			} // for each

			pc.Log();

			int row = 1 + DrawVerificationData(statSheet, 1, decisionStats);
			int loanIDColumn = 1;

			foreach (Tuple<Stats, int> pair in stats) {
				row = pair.Item1.ToXlsx(row);
				row++;

				loanIDColumn = pair.Item1.FlushLoanIDs(loanIDSheet, loanIDColumn);
			} // for each

			Xlsx.AutoFitColumns();
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

		protected virtual DateTime? DateTo {
			get { return this.spLoad.DateTo; }
			set { this.spLoad.DateTo = value; }
		} // DateTo

		private static int DrawVerificationData(ExcelWorksheet statSheet, int row, Stats decisionStats) {
			AStatItem.SetBorder(statSheet.Cells[row, 1, row, 3]).Merge = true;
			statSheet.SetCellValue(row, 1, "Verification data", bSetZebra: false, oBgColour: Color.Yellow, bIsBold: true);
			statSheet.Cells[row, 1].Style.Font.Size = 16;
			row++;

			statSheet.SetCellValue(row, 2, "Reference", true);
			statSheet.SetCellValue(row, 3, "Actual", true);
			row++;

			row = DrawVerificationRow(
				statSheet,
				row,
				"Approve count",
				Reference.Approve.Count,
				decisionStats.ManuallyApproved.Count,
				TitledValue.Format.Int
			);

			row = DrawVerificationRow(
				statSheet,
				row,
				"Approve amount",
				Reference.Approve.Amount,
				decisionStats.ManuallyApproved.Amount,
				TitledValue.Format.Money
			);

			row = DrawVerificationRow(
				statSheet,
				row,
				"Loan count",
				Reference.Loan.Count,
				decisionStats.ManuallyApproved.LoanCount.Total.Count,
				TitledValue.Format.Int
			);

			row = DrawVerificationRow(
				statSheet,
				row,
				"Loan amount",
				Reference.Loan.Amount,
				decisionStats.ManuallyApproved.LoanCount.Total.Amount,
				TitledValue.Format.Money
			);

			row = DrawVerificationRow(
				statSheet,
				row,
				"Default count",
				Reference.Default.Count,
				decisionStats.ManuallyApproved.LoanCount.DefaultIssued.Count,
				TitledValue.Format.Int,
				true
			);

			row = DrawVerificationRow(
				statSheet,
				row,
				"Default issued amount",
				Reference.Default.Issued.Amount,
				decisionStats.ManuallyApproved.LoanCount.DefaultIssued.Amount,
				TitledValue.Format.Money,
				true
			);

			row = DrawVerificationRow(
				statSheet,
				row,
				"Default outstanding amount",
				Reference.Default.Outstanding.Amount,
				decisionStats.ManuallyApproved.LoanCount.DefaultOutstanding.Amount,
				TitledValue.Format.Money,
				true
			);

			return row;
		} // DrawVerificationData

		private static int DrawVerificationRow(
			ExcelWorksheet statSheet,
			int row,
			string title,
			decimal reference,
			decimal actual,
			string format,
			bool setItalic = false
		) {
			Color fontColour = Math.Abs(reference - actual) < 0.000001m ? Color.DarkGreen : Color.Red;

			statSheet.SetCellValue(row, 1, title, true);
			statSheet.SetCellValue(row, 2, reference, sNumberFormat: format);
			statSheet.SetCellValue(row, 3, actual, oFontColour: fontColour, sNumberFormat: format);

			statSheet.Cells[row, 1, row, 3].Style.Font.Italic = setItalic;

			return row + 1;
		} // DrawVerificationRow

		private void LoadCashRequests() {
			Data.Clear();

			var byCustomer = new SortedDictionary<int, CustomerData>();

			ProgressCounter pc = new ProgressCounter("{0} cash requests loaded so far...", Log, 50);

			SetupFeeCalculator.ReloadBrokerRepoCache();

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
				customerData.FindLoansAndFilterAraOut(CashRequestLoans, LoanSources);
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

		private static class Reference {
			public static class Approve {
				public const int Count = 4621;
				public const decimal Amount = 45888566m;
			} // class Approve

			public static class Loan {
				public const int Count = 3938;
				public const decimal Amount = 37577566m;
			} // class Loan

			public static class Default {
				public const int Count = 319;

				public static class Issued {
					public const decimal Amount = 2567666m;
				} // class Issued

				public static class Outstanding {
					public const decimal Amount = 1651147m;
				} // class Outstanding
			} // class Default
		} // class Reference
	} // class MaamMedalAndPricing
} // namespace

