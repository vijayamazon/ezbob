// ReSharper disable UnusedMember.Local

namespace TestApp {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.Common;
	// using System.Data.SqlClient;
	using System.Diagnostics;
	using System.IO;
	using ConfigManager;
	using EzBob.Backend.Models;
	using EzBob.Backend.Strategies.Broker;
	using EzBob.Backend.Strategies.MainStrategy.AutoDecisions;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Security;
	using Ezbob.ValueIntervals;
	using Ezbob.Utils.Html;
	using JetBrains.Annotations;
	using Reports;
	using Reports.Alibaba.DataSharing;
	using Reports.Alibaba.Funnel;
	using Reports.EarnedInterest;
	using SqlConnection = Ezbob.Database.SqlConnection;

	class Program {
		private class A {
			public int F1 { get; set; }
			public int F2 { get; private set; }
			public virtual int V1 { get; set; }
			public virtual int V2 { get; private set; }
		} // class A

		private class B : A {
			public int F3 { get; set; }
			public int F4 { get; private set; }
			public virtual int V3 { get; set; }
			public virtual int V4 { get; private set; }
		} // class B

		#region method Main

		private static ASafeLog ms_oLog;

		static void Main(string[] args) {
			var log = new ConsoleLog(new LegacyLog());

			ms_oLog = log;

			var oDB = new SqlConnection(log);

			CurrentValues.Init(oDB, log);

			// TestBadPeriods(oDB, ms_oLog);

			// PropertyTraverser.Traverse<B>((oInstance, oInfo) => { ms_oLog.Msg("Instance is {0}", oInstance); ms_oLog.Msg("Property name is {0}", oInfo.Name); });

			// UpdateBrokerPasswords(oDB, log);

			// TestTableSpArgument(oDB, log);

			// TestVectorSpArgument(oDB, log);

			// TestRetryerWithArguments(oDB, log);

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

			// TestSpeed(oDB, log);

			// TestDataSharing(oDB, log);

			// TestAddBrokers(oDB, log);

			// TestReapproval(oDB, log);
		} // Main

		#endregion method Main

		#region method TestBadPeriods

		private static void TestBadPeriods(AConnection oDB, ASafeLog oLog) {
			oDB.LogVerbosityLevel = LogVerbosityLevel.Verbose;

			var bp = new BadPeriods(new DateTime(2001, 2, 12));

			bp.Add(new DateTime(2002, 3, 13), false);
			bp.Add(new DateTime(2002, 4, 18), false);
			bp.Add(new DateTime(2005, 5, 13), true);
			bp.Add(new DateTime(2006, 5, 13), false);

			oLog.Debug("{0}", bp);
		} // TestBadPeriods

		#endregion method TestBadPeriods

		#region method UpdateBrokerPasswords

		private static void UpdateBrokerPasswords(AConnection oDB, ASafeLog oLog) {
			var oQueries = new List<string>();

			oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					oQueries.Add(string.Format(
						"UPDATE Broker SET Password = '{0}' WHERE BrokerID = {1}",
						SecurityUtils.HashPassword((string)sr["ContactEmail"], "123456"),
						(int)sr["BrokerID"]
					));
					return ActionResult.Continue;
				},
				"SELECT BrokerID, ContactEmail FROM Broker",
				CommandSpecies.Text
			);

			oQueries.ForEach(sQuery => oDB.ExecuteNonQuery(sQuery, CommandSpecies.Text));
		} // UpdateBrokerPasswords

		#endregion method UpdateBrokerPasswords

		#region method TestTableSpArgument

		private static void TestTableSpArgument(AConnection oDB, ASafeLog oLog) {
			oDB.LogVerbosityLevel = LogVerbosityLevel.Verbose;

			var lst = new List<ConfigTable> {
				new ConfigTable { Start = 1, End = 2, Value = 1.2m, },
				new ConfigTable { Start = 2, End = 3, Value = 2.3m, },
				new ConfigTable { Start = 3, End = 4, Value = 3.4m, },
				new ConfigTable { Start = 4, End = 5, Value = 4.5m, },
				new ConfigTable { Start = 5, End = 6, Value = 5.6m, },
			};

			oLog.Debug("Results - begin:");

			oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					oLog.Debug("Returned value: {0}, {1}, {2}", (int)sr[0], (int)sr[1], (decimal)sr[2]);
					return ActionResult.Continue;
				},
				"TestIntIntDecimalListType",
				CommandSpecies.StoredProcedure,
				oDB.CreateTableParameter<ConfigTable>("@TheList", lst, objbir =>
				{
					var bir = (ConfigTable)objbir;
					return new object[] { bir.Start, bir.End, bir.Value, };
				})
			);

			oLog.Debug("Results - end");
		} // TestTableSpArgument

		#endregion method TestTableSpArgument

		#region method TestVectorSpArgument

		private static void TestVectorSpArgument(AConnection oDB, ASafeLog oLog) {
			oDB.LogVerbosityLevel = LogVerbosityLevel.Verbose;

			oLog.Debug("Results - begin:");

			oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					oLog.Debug("Returned value: {0}", (int)sr[0]);
					return ActionResult.Continue;
				},
				"TestIntListType",
				CommandSpecies.StoredProcedure,
				oDB.CreateVectorParameter<int>("@TheList", 1, 2, 2, 2, 5, 5, 38, 1)
			);

			oLog.Debug("Results - end");
		} // TestVectorSpArgument

		#endregion method TestVectorSpArgument

		#region method TestRetryerWithArguments

		private static void TestRetryerWithArguments(AConnection oDB, ASafeLog log) {
			var oRetryer = new SqlRetryer(nRetryCount: 8, nSleepBeforeRetryMilliseconds: 1000, oLog: log) {
				LogVerbosityLevel = oDB.LogVerbosityLevel
			};

			ms_nActionTestCounter = 0;

			Action oAction = () => ActionTest(28, "some string", oDB, log);

			oRetryer.Retry(oAction, "just a test action for 8 retries");

			var oTwoRetryer = new SqlRetryer(nRetryCount: 2, nSleepBeforeRetryMilliseconds: 1000, oLog: log) {
				LogVerbosityLevel = oDB.LogVerbosityLevel
			};

			ms_nActionTestCounter = 0;

			oTwoRetryer.Retry(oAction, "just a test action for 2 retries");
		} // TestRetryerWithArguments

		private static int ms_nActionTestCounter;

		private static void ActionTest(int x, string s, AConnection oDB, ASafeLog log) {
			log.Info("ActionTest started...");

			var sp = new UpdateBroker(oDB, log);
			sp.ExecuteNonQuery();

			/*

			var sp = new BrokerLoadCustomerList(oDB, log) { Email = "alexbo+broker@ezbob.com", };

			sp.ForEachResult<BrokerLoadCustomerList.ResultRow>(oRow => {
				log.Debug("Result row: {0}", oRow);
				return ActionResult.Continue;
			});
			*/

			if (ms_nActionTestCounter < 3) {
				ms_nActionTestCounter++;
				throw new ForceRetryException("just a force retry");
			} // if

			log.Info("ActionTest: x = {0}", x);
			log.Info("ActionTest: s = {0}", s);
			log.Info("ActionTest complete.");
		} // ActionTest

		private class UpdateBroker : AStoredProcedure {
			public UpdateBroker(AConnection oDB, ASafeLog oLog) : base(oDB, oLog, CommandSpecies.Text) {} // constructor

			public override bool HasValidParameters() { return true; }

			protected override string GetName() {
				return "UPDATE Broker SET FirmRegNum = '034343434' WHERE BrokerID = 2";
			}
		}

		private class BrokerLoadCustomerList : AStoredProcedure {
			public BrokerLoadCustomerList(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() { return true; }

			[FieldName("@ContactEmail")]
			public string Email { get; set; } // Email

			public class ResultRow : AResultRow {
				public int CustomerID { get; set; }
				public string FirstName { get; set; }
				public string LastName { get; set; }
				public string Email { get; set; }
				public string WizardStep { get; set; }
				public string Status { get; set; }
				public DateTime ApplyDate { get; set; }
				public string MpTypeName { get; set; }
				public string LoanAmount { get; set; }
				public DateTime LoanDate { get; set; }

				public override string ToString() {
					return string.Join(", ",
						CustomerID,
						FirstName,
						LastName,
						Email,
						WizardStep,
						Status,
						ApplyDate,
						MpTypeName,
						LoanAmount,
						LoanDate
					);
				} // ToString
			} // class ResultRow
		} // BrokerLoadCustomerList

		#endregion method TestRetryerWithArguments

		#region method TestEarnedInterestForAudit

		private static void TestEarnedInterestForAudit(AConnection oDB, ASafeLog log) {
			var ea = new EarnedInterest(
				oDB,
				EarnedInterest.WorkingMode.AccountingLoanBalance,
				false,
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
			oLog.Info("Using row - begin");

			oDB.ForEachRowSafe((sr, bRowsetStart) => {
				int nCustomerID = sr["Id"];
				string sName = sr["Name"];
				bool bIsOffline = sr[2];
				DateTime dt = sr["GreetingMailSentDate", new DateTime(2014, 12, 12)];

				oLog.Info("{0}: {1} - {2} {3}", nCustomerID, sName, bIsOffline, dt);

				return ActionResult.Continue;
			}, "SELECT Id, Name, IsOffline, GreetingMailSentDate FROM Customer ORDER BY Id", CommandSpecies.Text);

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
			var oPeriods = new SortedDictionary<int, InterestFreezePeriods>();

			oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nLoanID = sr["LoanId"];
					DateTime? oStart = sr["StartDate"];
					DateTime? oEnd = sr["EndDate"];
					decimal nRate = sr["InterestRate"];
					DateTime? oDeactivation = sr["DeactivationDate"];

					DateTime? oTo = oDeactivation.HasValue
						? (oEnd.HasValue ? DateInterval.Min(oEnd.Value, oDeactivation.Value) : oDeactivation)
						: oEnd;

					if (!oPeriods.ContainsKey(nLoanID))
						oPeriods[nLoanID] = new InterestFreezePeriods();

					oPeriods[nLoanID].Add(oStart, oTo, nRate);

					return ActionResult.Continue;
				},
				"RptEarnedInterest_Freeze",
				CommandSpecies.StoredProcedure
			);

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
				false,
				new DateTime(2012, 9, 1),
				new DateTime(2018, 3, 8),
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

		/*

		#region method TestSpeed

		private static void TestSpeed(AConnection oDB, ASafeLog oLog) {
			const int nCount = 1000;

			const string sUserName = "alexbo+broker@ezbob.com";
			const string sPassword = "9843d8f274996c952b7ad5f4f3553604d94197d10b73566d756b5df6e2af8ea1424294eb8b9cab2170edd0f7f0e9b2617863ef0b4800749eec898aa2d03f3bba";

			var swi = Stopwatch.StartNew();

			for (int i = 0; i < nCount; i++) {
				DataTable tbl = oDB.ExecuteReader(
					"BrokerLogin",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@Email", sUserName),
					new QueryParameter("@Password", sPassword)
				);

				if (tbl.Rows.Count > 0) {
					SafeReader sr = new SafeReader(tbl.Rows[0]);

					BrokerProperties properties = new BrokerProperties {
						BrokerID = sr["BrokerID"],
						BrokerName = sr["BrokerName"],
						BrokerRegNum = sr["BrokerRegNum"],
						ContactName = sr["ContactName"],
						ContactEmail = sr["ContactEmail"],
						ContactMobile = sr["ContactMobile"],
						ContactOtherPhone = sr["ContactOtherPhone"],
						SourceRef = sr["SourceRef"],
						BrokerWebSiteUrl = sr["BrokerWebSiteUrl"],
						SignedTermsID = sr["SignedTermsID"],
						SignedTextID = sr["SignedTextID"],
						CurrentTermsID = sr["CurrentTermsID"],
						CurrentTextID = sr["CurrentTextID"],
						CurrentTerms = sr["CurrentTerms"]
					};

					properties.BrokerID++;
				}
				else
					oLog.Debug("No first row.");
			} // for

			swi.Stop();

			oLog.Info("{0} times using ExecuteReader: {1} ms.", nCount, swi.ElapsedMilliseconds);

			var sw = Stopwatch.StartNew();

			for (int i = 0; i < nCount; i++) {
				BrokerProperties properties = new BrokerProperties(); 

				var sp = new SpBrokerLogin(oDB, oLog) {
					Email = sUserName,
					Password = sPassword,
				};

				sp.FillFirst(properties);

				properties.BrokerID++;
			} // for

			sw.Stop();

			oLog.Info("{0} times using GetFirst: {1} ms.", nCount, sw.ElapsedMilliseconds);
		} // TestSpeed

		private class SpBrokerLogin : AStoredProc {
			#region constructor

			public SpBrokerLogin(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			#endregion constructor

			#region method HasValidParameters

			public override bool HasValidParameters() {
				Email = MiscUtils.ValidateStringArg(Email, "Email");
				Password = MiscUtils.ValidateStringArg(m_sPassword, "Password");

				return true;
			} // HasValidParameters

			#endregion method HasValidParameters

			#region property Email

			[UsedImplicitly]
			public string Email { get; set; }

			#endregion property Email

			#region property Password

			public string Password {
				[UsedImplicitly]
				get { return SecurityUtils.HashPassword(Email, m_sPassword); }
				set { m_sPassword = value; }
			} // Password

			private string m_sPassword;

			#endregion property Password
		} // class SpBrokerLogin

		#endregion method TestSpeed

		*/

		#region method TestDataSharing

		private static void TestDataSharing(AConnection oDB, ASafeLog oLog) {
			DataSharing ds = new DataSharing(false, oDB, oLog);
			ds.Generate();

			ds.Report.SaveAs(new FileInfo(@".\data\data_sharing_test.xlsx"));

			FunnelCreator f = new FunnelCreator(null, oDB, oLog);
			f.Generate();

			f.Report.SaveAs(new FileInfo(@".\data\funnel_test.xlsx"));
		} // TestDataSharing

		#endregion method TestDataSharing

		#region method TestAddBrokers

		private static void TestAddBrokers(AConnection oDB, ASafeLog oLog) {
			var oBrokers = new List<BrokerData> {
			// In Excel:
			// =CONCATENATE("new BrokerData(""",F2, """, """, A2,""", """, B2, """, """, C2, """, """, D2, """, """, E2, """),")
			};

			foreach (BrokerData bd in oBrokers) {
				try {
					bd.Signup(oDB, oLog);
				}
				catch (Exception e) {
					oLog.Alert(e, "Failed to create a broker {0}", bd.Email);
				} // try
			} // for each
		} // TestAddBrokers

		#endregion method TestAddBrokers

		#region method TestReapproval

		private static void TestReapproval(AConnection oDB, ASafeLog oLog) {
			var adr = new AutoDecisionResponse();

			var ra = new EzBob.Backend.Strategies.MainStrategy.AutoDecisions.ReApproval.Agent(339, oDB, oLog).Init();

			ra.MakeDecision(adr);
		} // TestReapproval

		#endregion method TestReapproval
	} // class Program

	#region class BrokerData

	internal class BrokerData {
		public BrokerData(string sEmail, string sFirstName, string sLastName, string sCompany, string sPrimaryPhone, string sOtherPhone) {
			FirstName = sFirstName;
			LastName = sLastName;
			Company = sCompany;
			PrimaryPhone = sPrimaryPhone;
			OtherPhone = sOtherPhone;
			Email = sEmail;
		} // BrokerData

		public string FirstName { get; private set; }
		public string LastName { get; private set; }
		public string Company { get; private set; }
		public string PrimaryPhone { get; private set; }
		public string OtherPhone { get; private set; }
		public string Email { get; private set; }

		private string ContactName {
			get { return 
				(FirstName.Trim() + " " + LastName.Trim()).Trim();
			} // get
		} // ContactName

		public void Signup(AConnection oDB, ASafeLog oLog) {
			var stra = new BrokerSignup(
				Company,
				string.Empty, // sFirmRegNum,
				ContactName,
				Email,
				PrimaryPhone,
				"222222",
				OtherPhone,
				10000m, // decimal nEstimatedMonthlyClientAmount,
				new Password("123456", "123456"),
				string.Empty, // string sFirmWebSiteUrl,
				5, // int nEstimatedMonthlyApplicationCount,
				true, // bool bIsCaptchaEnabled,
				6, // int nBrokerTermsID,
				"Finance Professional Show - Nov 2014", // string sReferredBy,
				oDB,
				oLog
			);

			stra.Execute();
		} // Signup
	} // class BrokerData

	#endregion class BrokerData
} // namespace

// ReSharper restore UnusedMember.Local
