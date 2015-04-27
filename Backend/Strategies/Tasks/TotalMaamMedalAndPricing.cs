namespace Ezbob.Backend.Strategies.Tasks {
	using System;
	using System.Globalization;
	using System.IO;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;

	public class TotalMaamMedalAndPricing : MaamMedalAndPricing {
		public TotalMaamMedalAndPricing(bool testMode) {
			this.testMode = testMode;
		} // constructor

		public override string Name {
			get { return "TotalMaamMedalAndPricing"; }
		} // Name

		public override void Execute() {
			RequestedCustomers.Clear();

			if (this.testMode) {
				RequestedCustomers.AddRange(new[] {
					1105, 211, 1001, 1884, 1934, 2058, 17304, 21285, 23583,
				});

				Log.Debug("Test mode, customer list: {0}.", string.Join(", ", RequestedCustomers));
			} // if

			DateFrom = new DateTime(2012, 9, 4, 0, 0, 0, DateTimeKind.Utc);
			DateTo   = new DateTime(2015, 4, 1, 0, 0, 0, DateTimeKind.Utc);

			base.Execute();

			SaveXlsx(string.Format(
				"automation.report.{0}.xlsx",
				DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
			));
		} // Execute

		private void SaveXlsx(string fileName) {
			string filePath = Path.Combine(Path.GetTempPath(), fileName);

			try {
				Xlsx.SaveAs(new FileInfo(filePath));
				Log.Debug("Saved .xlsx file as {0}.", filePath);
			} catch (Exception e) {
				Log.Warn(e, "Failed to save .xlsx file as {0}.", filePath);
			} // try
		} // CreateXlsx

		private readonly bool testMode;
	} // class WeeklyMaamMedalAndPricing
} // namespace

