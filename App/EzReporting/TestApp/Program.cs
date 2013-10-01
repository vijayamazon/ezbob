using System;
using Ezbob.Database;
using Ezbob.Logger;
using Ezbob.ValueIntervals;
using Html;
using Reports;

namespace TestApp {

	class A { public static string func() {
		return "this is A";
	} }

	class B : A { public static string func() {
		return "this is B";
	} }

	class Program {
		static void Main(string[] args) {
			var log = new ConsoleLog(new LegacyLog());

			var oDB = new SqlConnection(log);

			// TestLoansIssuedReport(oDB, log);
			// TestEarnedInterest(oDB, log);
			// TestLoanIntegrity(oDB, log);

			// TestLoanStats(oDB, log);

			var di2 = new DateInterval(new DateTime(1976, 7, 1), null);
			var di = new DateInterval(null, new DateTime(1982, 9, 1));

			Console.WriteLine("{0} ^ {1} = {2}: {3}", di, di2, di.Intersects(di2) ? "yes" : "no", di.Intersection(di2));

			var e1 = new DateIntervalEdge(null, AIntervalEdge<DateTime>.EdgeType.NegativeInfinity);
			var e2 = new DateIntervalEdge(new DateTime(1976, 7, 1), AIntervalEdge<DateTime>.EdgeType.Finite);

			Console.WriteLine("{0} > {1} = {2}", e1, e2, e1 > e2);

			Console.WriteLine("A.func() = {0}", A.func());
			Console.WriteLine("B.func() = {0}", B.func());
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
