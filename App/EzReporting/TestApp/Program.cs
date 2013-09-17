﻿using System;
using Ezbob.Database;
using Ezbob.Logger;
using Html;
using Reports;

namespace TestApp {
	class Program {
		static void Main(string[] args) {
			var log = new ConsoleLog(new LegacyLog());

			var oDB = new SqlConnection(log);

			// TestLoansIssuedReport(oDB, log);
			// TestEarnedInterest(oDB, log);
			// TestLoanIntegrity(oDB, log);

			TestLoanStats(oDB, log);
		} // Main

		private static void TestLoanStats(AConnection oDB, ASafeLog log) {
			var sender = new ReportDispatcher(oDB, log);
			sender.Dispatch("loan_stats", DateTime.Today, null, new LoanStats(oDB, log).Xls(), ReportDispatcher.ToDropbox);
		} // TestLoanStats

		private static void TestLoansIssuedReport(AConnection oDB, ASafeLog log) {
			var brh = new BaseReportHandler(oDB, log);

			var rpt = new Report(oDB, Reports.ReportType.RPT_LOANS_GIVEN.ToString());

			ATag oTag = brh.BuildLoansIssuedReport(rpt, new DateTime(2013, 7, 1), new DateTime(2013, 7, 15));
		} // TestLoansIssuedReport

		private static void TestEarnedInterest(AConnection oDB, ASafeLog log) {
			var ea = new EarnedInterest(
				oDB,
				EarnedInterest.WorkingMode.ByIssuedLoans,
				new DateTime(2012, 5, 1),
				DateTime.Today.AddDays(1),
				log
			) {
				VerboseLogging = true
			};

			ea.Run();
		} // TestEarnedInterest

		private static void TestLoanIntegrity(AConnection oDB, ASafeLog log) {
			var ea = new LoanIntegrity(oDB, log) {
				VerboseLogging = true
			};

			ea.Run();
		} // TestLoanIntegrity
	} // class Program
} // namespace
