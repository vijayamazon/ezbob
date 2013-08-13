using System;
using System.Globalization;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzReportToEMail {
	public class Program {
		public static void Main(string[] args) {
			var log = new LegacyLog();
			var env = new Ezbob.Context.Environment(log);
/*
			var ea = new Reports.EarnedInterest(
				new SqlConnection(log),
				Reports.EarnedInterest.WorkingMode.ByIssuedLoans,
				new DateTime(2012, 5, 1),
				new DateTime(2013, 9, 1),
				log
			);

			var eaint = ea.Run();

			foreach (var d in eaint) {
				log.Debug("{0}: {1}", d.Key, d.Value);
			}

			return;
*/
			DateTime dNow = DateTime.Today;

			if ((args.Length == 1) && (args[0] == "--std"))
				dNow = dNow.AddDays(-1);
			else if ((args.Length > 1) && (args[0] == "--date"))
				DateTime.TryParseExact(args[1], "yyyy-MM-dd", new CultureInfo("en-GB"), DateTimeStyles.None, out dNow);

			log.Info("Current environment is {0}", env.Context);
			log.Info("Running with current date {0}", dNow.ToString("MMMM d yyyy H:mm:ss"));

			var reportsHandler = new EmailReportHandler(new SqlConnection(log), log);
			reportsHandler.ExecuteReportHandler(dNow);
		} // Main
	} // class Program
} // namespace EzReportToEmail
