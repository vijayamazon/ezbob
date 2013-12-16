using System;
using System.Collections.Generic;
using System.Data;
using Ezbob.Database;
using Ezbob.Logger;
using Ezbob.ValueIntervals;
using Html;
using Reports;

namespace TestApp {
	class Program {
		#region method Main

		static void Main(string[] args) {
			var log = new ConsoleLog(new LegacyLog());

			var oDB = new SqlConnection(log);

			TestExperianLimitedCompanyData(oDB, log);

			// TestUiReport(oDB, log);

			// TestLoansIssuedReport(oDB, log);

			// TestEarnedInterest(oDB, log);

			// TestLoanIntegrity(oDB, log);

			// TestLoanStats(oDB, log);

			// TestIntervalsOperations();

			// TestInterestFreeze(oDB, log);
		} // Main

		#endregion method Main

		#region method TestExperianLimitedCompanyData

		private static void TestExperianLimitedCompanyData(AConnection oDB, ASafeLog log) {
			var rpt = new ExperianLimitedCompanyData(oDB, log);
			rpt.VerboseLogging = true;

			Tuple<List<ExperianLimitedCompanyReportItem>, SortedSet<string>> oOutput = rpt.Run();

			log.Debug("Report start");

			ExperianLimitedCompanyData.ToOutput(@"c:\temp\dl99.csv", oOutput);

			log.Debug("Report end");
		} // TestExperianLimitedCompanyData

		#endregion method TestExperianLimitedCompanyData

		#region method TestUiReport

		private static void TestUiReport(AConnection oDB, ASafeLog log) {
			var rpt = new UiReport(oDB, new DateTime(2013, 12, 1), new DateTime(2013, 12, 10), log);
			rpt.VerboseLogging = true;

			SortedDictionary<int, UiReportItem> oOutput = rpt.Run();

			log.Debug("Report start");

			foreach (KeyValuePair<int, UiReportItem> pair in oOutput)
				log.Debug(pair.Value.ToString());

			log.Debug("Report end");
		} // TestUiReport

		#endregion method TestUiReport

		#region method TestInterestFreeze

		private static void TestInterestFreeze(AConnection oDB, ASafeLog log) {
			DataTable tbl = oDB.ExecuteReader("RptEarnedInterest_Freeze", CommandSpecies.StoredProcedure);
			
			var oPeriods = new SortedDictionary<int, InterestFreezePeriods>();

			foreach (DataRow row in tbl.Rows) {
				int nLoanID = Convert.ToInt32(row["LoanId"]);
				DateTime? oStart = row["StartDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["StartDate"]);
				DateTime? oEnd = row["EndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["EndDate"]);
				decimal nRate = Convert.ToDecimal(row["InterestRate"]);
				DateTime? oDeactivation = row["DeactivationDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["DeactivationDate"]);

				DateTime? oTo = oDeactivation.HasValue
					? (oEnd.HasValue ? DateInterval.Min(oEnd.Value, oDeactivation.Value) : oDeactivation)
					: oEnd;

				if (!oPeriods.ContainsKey(nLoanID))
					oPeriods[nLoanID] = new InterestFreezePeriods();

				oPeriods[nLoanID].Add(oStart, oTo, nRate);
			} // for each

			foreach (var pair in oPeriods)
				log.Msg("LoanID: {0} Freeze Periods: {1}", pair.Key, pair.Value);
		} // TestInerestFreeze

		#endregion method TestInterestFreeze

		#region method TestIntervalsOperations

		private static void TestIntervalsOperations() {
			TestIntervalsOperations(
				new FreezeInterval(new DateTime(1976, 7, 1), null, 0.3m),
				new FreezeInterval(null, new DateTime(1982, 9, 1), 0.6m)
			);

			TestIntervalsOperations(
				new FreezeInterval(new DateTime(1976, 7, 1), new DateTime(1982, 9, 1), 0.3m),
				new FreezeInterval(new DateTime(1979, 5, 9), new DateTime(1979, 11, 14), 0.6m)
			);

			TestIntervalsOperations(
				new FreezeInterval(new DateTime(1976, 7, 1), new DateTime(1982, 9, 1), 0.3m),
				new FreezeInterval(new DateTime(1979, 5, 9), new DateTime(1979, 5, 9), 0.6m)
			);
		} // TestIntervalOperations

		#endregion method TestIntervalsOperations

		#region method TestIntervalsOperations

		private static void TestIntervalsOperations(FreezeInterval di, FreezeInterval di2) {
			Console.WriteLine("\n***\n\n");

			Console.WriteLine("{0} ^ {1} = {2}", di, di2, di * di2);
			Console.WriteLine("{0} - {1} = {2}", di, di2, di - di2);

			Console.WriteLine("---");

			Console.WriteLine("{0} ^ {1} = {2}", di2, di, di2 * di);
			Console.WriteLine("{0} - {1} = {2}", di2, di, di2 - di);

			Console.WriteLine("\n***\n\n");
		} // TestIntervalOperations

		#endregion method TestIntervalsOperations

		#region method TestLoanStats

		private static void TestLoanStats(AConnection oDB, ASafeLog log) {
			var sender = new ReportDispatcher(oDB, log);
			sender.Dispatch("loan_stats", DateTime.Today, null, new LoanStats(oDB, log).Xls(), ReportDispatcher.ToDropbox);
		} // TestLoanStats

		#endregion method TestLoanStats

		#region method TestLoansIssuedReport

		private static void TestLoansIssuedReport(AConnection oDB, ASafeLog log) {
			var brh = new BaseReportHandler(oDB, log);

			var rpt = new Report(oDB, Reports.ReportType.RPT_LOANS_GIVEN.ToString());

			ATag oTag = brh.BuildLoansIssuedReport(rpt, new DateTime(2013, 7, 1), new DateTime(2013, 7, 15));
		} // TestLoansIssuedReport

		#endregion method TestLoansIssuedReport

		#region method TestEarnedInterest

		private static void TestEarnedInterest(AConnection oDB, ASafeLog log) {
			var ea = new EarnedInterest(
				oDB,
				EarnedInterest.WorkingMode.ByIssuedLoans,
				new DateTime(2013, 3, 7),
				new DateTime(2013, 3, 8),
				log
			) {
				VerboseLogging = true
			};

			ea.Run();
		} // TestEarnedInterest

		#endregion method TestEarnedInterest

		#region method TestLoanIntegrity

		private static void TestLoanIntegrity(AConnection oDB, ASafeLog log) {
			var ea = new LoanIntegrity(oDB, log) {
				VerboseLogging = true
			};

			ea.Run();
		} // TestLoanIntegrity

		#endregion method TestLoanIntegrity
	} // class Program
} // namespace
