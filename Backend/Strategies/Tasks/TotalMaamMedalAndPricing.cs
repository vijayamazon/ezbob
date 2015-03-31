namespace Ezbob.Backend.Strategies.Tasks {
	using System;
	using System.Globalization;
	using System.IO;
	using Ezbob.Backend.Strategies.AutomationVerification.KPMG;

	public class TotalMaamMedalAndPricing : MaamMedalAndPricing {
		public override string Name {
			get { return "TotalMaamMedalAndPricing"; }
		} // Name

		public override void Execute() {
			CustomerID = 1001; // TODO: remove!

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
	} // class WeeklyMaamMedalAndPricing
} // namespace

