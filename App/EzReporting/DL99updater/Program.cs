namespace DL99updater {
	using System;
	using System.Collections.Generic;
	using Ezbob.Context;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Reports;

	class Program {

		static void Main(string[] args) {
			var log = new ConsoleLog(new LegacyLog());

			var env = new Ezbob.Context.Environment(Name.Production, oLog: log);
			var oDB = new SqlConnection(env, log);

			RunExperianLimitedCompanyData(oDB, log);
			RunLoanDateScore(oDB, log);
		} // Main

		private static void RunLoanDateScore(AConnection oDB, ASafeLog log) {
			var rpt = new LoanDateScore(oDB, log) { VerboseLogging = true };

			rpt.Run();

			log.Debug("Report start");

			rpt.ToOutput(@"c:\temp\loan_date_score.csv");

			log.Debug("Report end");
		} // RunLoanDateScore

		private static void RunExperianLimitedCompanyData(AConnection oDB, ASafeLog log) {
			var rpt = new ExperianLimitedCompanyData(oDB, log) { VerboseLogging = true };

			Tuple<List<ExperianLimitedCompanyReportItem>, SortedSet<string>> oOutput = rpt.Run();

			log.Debug("Report start");

			ExperianLimitedCompanyData.ToOutput(@"c:\temp\dl99.csv", oOutput);

			log.Debug("Report end");
		} // RunExperianLimitedCompanyData

	} // class Program
} // namespace
