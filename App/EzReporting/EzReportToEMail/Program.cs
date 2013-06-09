using System;
using System.Globalization;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzReportToEMail {
	public class Program {
		public static void Main(string[] args) {
			var log = new LegacyLog();
			var env = new Ezbob.Context.Environment(log);

			DateTime dNow = DateTime.Now;

			if ((args.Length > 1) && (args[0] == "--date"))
				DateTime.TryParseExact(args[1], "yyyy-MM-dd", new CultureInfo("en-GB"), DateTimeStyles.None, out dNow);

			log.Info("Current environment is {0}", env.Context);
			log.Info("Running with current date {0}", dNow.ToString("yyyy-MM-dd"));

			var reportsHandler = new EmailReportHandler(new SqlConnection(log), log);
			reportsHandler.ExecuteReportHandler(dNow);
		} // Main
	} // class Program
} // namespace EzReportToEmail
