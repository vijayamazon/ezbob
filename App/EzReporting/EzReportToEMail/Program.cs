namespace EzReportToEMail {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Reports;

	public class Program {
		public static void Main(string[] args) {
			var log = new LegacyLog();
			var env = new Ezbob.Context.Environment(log);

			DateTime dNow = DateTime.Today;

			var oArgs = new Queue<string>(args);

			ReportType? nReportToExecute = null;

			bool bStdSet = false;
			bool bDateSet = false;

			while (oArgs.Count > 0) {
				string sArg = oArgs.Dequeue();

				switch (sArg) {
				case "--std":
					bStdSet = true;
					dNow = dNow.AddDays(-1);
					break;

				case "--date":
					bDateSet = true;
					string sDate = (oArgs.Count > 0) ? oArgs.Dequeue() : string.Empty;
					DateTime.TryParseExact(sDate, "yyyy-MM-dd", new CultureInfo("en-GB"), DateTimeStyles.None, out dNow);
					break;

				case "--report":
					if (oArgs.Count > 0) {
						ReportType nRptType;
						if (Enum.TryParse(oArgs.Dequeue(), true, out nRptType))
							nReportToExecute = nRptType;
					} // if
					break;
				} // switch
			} // while

			if (bDateSet && bStdSet) {
				log.Fatal("Both --std and --date set. There can be  only one!");
				throw new Exception("Both --std and --date set. There can be  only one!");
			} // if

			log.Info("Report delivery daemon started...");

			log.Info("Current environment is {0}", env.Context);
			log.Info("Running with current date {0}", dNow.ToString("MMMM d yyyy H:mm:ss"));

			var reportsHandler = new EmailReportHandler(new SqlConnection(log), log);
			reportsHandler.ExecuteReportHandler(dNow, nReportToExecute);

			log.Info("Report delivery daemon completed all the tasks.");
		} // Main
	} // class Program
} // namespace EzReportToEmail
