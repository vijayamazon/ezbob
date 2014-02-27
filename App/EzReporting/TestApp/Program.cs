﻿// ReSharper disable UnusedMember.Local

namespace TestApp {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.Common;
	using System.Data.SqlClient;

	using Ezbob.Context;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.ValueIntervals;
	using Html;
	using Reports;
	using SqlConnection = Ezbob.Database.SqlConnection;

	class Program {
		#region method Main

		private static ASafeLog ms_oLog;

		static void Main(string[] args) {
			var log = new ConsoleLog(new LegacyLog());

			ms_oLog = log;

			var oDB = new SqlConnection(log);
			oDB.LogVerbosityLevel = LogVerbosityLevel.Verbose;

			TestRetryerWithArguments(oDB, log);

			// TestEarnedInterestForAudit(oDB, log);

			// TestHashPassword(oDB, log);

			// TestParsedValues(oDB, log);

			// TestUiReportExt(oDB, log);

			// TestLoanDateScore(oDB, log);

			// TestExperianLimitedCompanyData(oDB, log);

			// TestUiReport(oDB, log);

			// TestLoansIssuedReport(oDB, log);

			// TestEarnedInterest(oDB, log);

			// TestLoanIntegrity(oDB, log);

			// TestLoanStats(oDB, log);

			// TestIntervalsOperations();

			// TestInterestFreeze(oDB, log);
		} // Main

		#endregion method Main

		#region private static TestRetryerWithArguments

		private static void TestRetryerWithArguments(AConnection oDB, ASafeLog log) {
			var oRetryer = new SqlRetryer(nRetryCount: 8, nSleepBeforeRetryMilliseconds: 2000, oLog: log) {
				LogVerbosityLevel = oDB.LogVerbosityLevel
			};

			ms_nActionTestCounter = 0;

			Action oAction = () => ActionTest(28, "some string", log);

			oRetryer.Retry(oAction, "just a test action for 8 retries");

			var oTwoRetryer = new SqlRetryer(nRetryCount: 2, nSleepBeforeRetryMilliseconds: 2000, oLog: log) {
				LogVerbosityLevel = oDB.LogVerbosityLevel
			};

			ms_nActionTestCounter = 0;

			oTwoRetryer.Retry(oAction, "just a test action for 2 retries");
		} // TestRetryerWithArguments

		private static int ms_nActionTestCounter;

		private static void ActionTest(int x, string s, ASafeLog log) {
			log.Info("ActionTest started...");

			if (ms_nActionTestCounter < 3) {
				ms_nActionTestCounter++;
				throw new ForceRetryException("just a force retry");
			} // if

			log.Info("ActionTest: x = {0}", x);
			log.Info("ActionTest: s = {0}", s);
			log.Info("ActionTest complete.");
		} // ActionTest

		#endregion private static TestRetryerWithArguments

		#region method TestEarnedInterestForAudit

		private static void TestEarnedInterestForAudit(AConnection oDB, ASafeLog log) {
			var ea = new EarnedInterest(
				oDB,
				EarnedInterest.WorkingMode.AccountingLoanBalance,
				new DateTime(2014, 1, 15),
				new DateTime(2014, 2, 15),
				log
			) {
				VerboseLogging = true
			};

			ea.Run();
		} // TestEarnedInterestForAudit

		#endregion method TestEarnedInterest

		#region method TestHashPassword

		private static void TestHashPassword(AConnection oDB, ASafeLog oLog) {
			var aryPasswords = new string[] {
				null, "", " ",
				"123456",
				"jkkasjzdhfkjjk",
				"lskdfsdlkjfsldkfjsldkfjsldkfjsldkjfsldkfjsdlkfjjdsfsldkjfsldkfjsldkfj",
				"дер.пароль",
				"סיסמא סידית ביותר",
				"email+123@example.com",
			};

			foreach (string sPassword in aryPasswords) {
				string sHash = Ezbob.Utils.Security.SecurityUtils.HashPassword(sPassword);
				oLog.Msg("Password: '{0}', hash size: {1}, hash: {2}\n", sPassword, sHash.Length, sHash);
			} // for each password
		} // TestHashPassword

		#endregion method TestHashPassword

		#region method TestParsedValues

		private static void TestParsedValues(AConnection oDB, ASafeLog oLog) {
			DataTable tbl = oDB.ExecuteReader("SELECT Id, Name, IsOffline, GreetingMailSentDate FROM Customer ORDER BY Id", CommandSpecies.Text);

			oLog.Info("Using row - begin");

			foreach (DataRow row in tbl.Rows) {
				var sr = new SafeReader(row);

				int nCustomerID = sr["Id"];
				string sName = sr["Name"];
				bool bIsOffline = sr[2];
				DateTime dt = sr["GreetingMailSentDate", new DateTime(2014, 12, 12)];

				oLog.Info("{0}: {1} - {2} {3}", nCustomerID, sName, bIsOffline, dt);
			} // foreach

			tbl.Dispose();

			oLog.Info("Using row - end");

			oLog.Info("Using reader - begin");
			oDB.ForEachRow(TestParsedValuesPrint, "SELECT Id, Name, IsOffline, GreetingMailSentDate FROM Customer ORDER BY Id", CommandSpecies.Text);
			oLog.Info("Using reader - end");
		} // TestParsedValues

		private static ActionResult TestParsedValuesPrint(DbDataReader row, bool bRowsetStarts) {
			var sr = new SafeReader(row);

			int nCustomerID = sr["Id"];
			string sName = sr["Name"];
			bool bIsOffline = sr["IsOffline"];
			DateTime dt = sr["GreetingMailSentDate"];

			ms_oLog.Info("{0}: {1} - {2} {3}", nCustomerID, sName, bIsOffline, dt);

			return ActionResult.Continue;
		} // TestParsedValuesPrint

		#endregion method TestParsedValues

		#region method TestUiReportExt

		private static void TestUiReportExt(AConnection oDB, ASafeLog log) {
			var rpt = new UiReportExt(oDB, new DateTime(2013, 12, 23), new DateTime(2013, 12, 31), log) { VerboseLogging = true };

			rpt.Run();

			log.Debug("Report start");

			log.Debug("Report end");
		} // TestUiReportExt

		#endregion method TestUiReportExt

		#region method TestLoanDateScore

		private static void TestLoanDateScore(AConnection oDB, ASafeLog log) {
			var rpt = new LoanDateScore(oDB, log) { VerboseLogging = true };

			rpt.Run();

			log.Debug("Report start");

			rpt.ToOutput(@"c:\temp\loan_date_score.csv");

			log.Debug("Report end");
		} // TestLoanDateScore

		#endregion method TestExperianLimitedCompanyData

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

			List<UiReportItem> oOutput = rpt.Run();

			log.Debug("Report start");

			foreach (UiReportItem oItem in oOutput)
				log.Debug(oItem.ToString());

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

// ReSharper restore UnusedMember.Local
