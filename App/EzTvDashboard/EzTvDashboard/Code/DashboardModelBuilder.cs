namespace EzTvDashboard.Code {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Database;
	using GoogleAnalyticsLib;
	using Models;

	public class DashboardModelBuilder {
		private AConnection Db { get; set; }
		private static string GaCertThumb { get; set; }
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(DashboardModelBuilder));
		private static DashboardModel DashboardModel { get; set; }

		private static int CustomerId { get; set; }
		private static int CashRequestsId { get; set; }
		private static int LoanId { get; set; }
		private static int LoanTransactionId { get; set; }
		private static int BrokerId { get; set; }
		public static DateTime LastChanged { get; set; }

		public DashboardModelBuilder() {
			Db = new SqlConnection();
			GaCertThumb = System.Configuration.ConfigurationManager.AppSettings["gaCertThumb"];
			DashboardModel = DashboardModel ?? BuildModel();
		}

		public bool SomethingChanged() {
			bool isCustomerChanged = false;
			bool isCashRequestChanged = false;
			bool isLoanChanged = false;
			bool isLoanTransactionChanged = false;
			bool isBrokerChanged = false;

			var changed = Db.ExecuteEnumerable("EzTvIsChanged");

			foreach (SafeReader row in changed) {
				int val;
				string sTableName = row["Table"];

				if (sTableName == "Security_User") {
					val = row["Val"];
					isCustomerChanged = val > CustomerId;
					CustomerId = val;
					continue;
				}
				if (sTableName == "CashRequests") {
					val = row["Val"];
					isCashRequestChanged = val > CashRequestsId;
					CashRequestsId = val;
					continue;
				}
				if (sTableName == "Loan") {
					val = row["Val"];
					isLoanChanged = val > LoanId;
					LoanId = val;
					continue;
				}
				if (sTableName == "LoanTransaction") {
					val = row["Val"];
					isLoanTransactionChanged = val > LoanTransactionId;
					LoanTransactionId = val;
					continue;
				}
				if (sTableName == "Broker") {
					val = row["Val"];
					isBrokerChanged = val > BrokerId;
					BrokerId = val;
					continue;
				}
			}

			bool somethingChanged = isCustomerChanged || isCashRequestChanged || isLoanChanged || isLoanTransactionChanged || isBrokerChanged;
			if (somethingChanged) {
				Log.DebugFormat("Something changed, building model customer:{0}, cash request:{1}, loan:{2}, loan transaction:{3}, broker:{4}",
						  isCustomerChanged, isCashRequestChanged, isLoanChanged, isLoanTransactionChanged, isBrokerChanged);
				DashboardModel = BuildModel();
			}

			return true;
		}

		public DashboardModel GetModel() {
			return DashboardModel;
		}

		private DashboardModel BuildModel() {
			LastChanged = DateTime.UtcNow;
			var model = new DashboardModel {
				Stats = new Dictionary<string, decimal>(),
			};

			var firstOfMonth = new DateTime(LastChanged.Year, LastChanged.Month, 1);
			Db.ForEachRow(
				(oReader, bRowsetStart) => {
					model.Stats.Add(oReader["Key"].ToString(), (decimal)oReader["Value"]);
					return ActionResult.Continue;
				}
				,
				"EzTvGetStats",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@Now", LastChanged),
				new QueryParameter("@FirstOfMonth", firstOfMonth),
				new QueryParameter("@MonthAgo", DateTime.Today.AddMonths(-1))
			);

			try {
				var ga = new GoogleAnalytics();
				ga.Init(DateTime.UtcNow, GaCertThumb);
				var todayAnalytics = ga.FetchByCountry(DateTime.Today, LastChanged);
				var monthToDateAnalytics = ga.FetchByCountry(firstOfMonth, LastChanged);
				if (todayAnalytics.ContainsKey("United Kingdom")) {
					model.Stats["T_UkVisitors"] = todayAnalytics["United Kingdom"].Users;
				}

				if (monthToDateAnalytics.ContainsKey("United Kingdom")) {
					model.Stats["M_UkVisitors"] = monthToDateAnalytics["United Kingdom"].Users;
				}
			} catch (Exception ex) {
				Log.ErrorFormat("Failed to load google analytics values \n{0}", ex);
			}

			return model;
		}


		public DashboardModel BuildFakeModel() {
			LastChanged = DateTime.UtcNow;
			var model = new DashboardModel {
				Stats = new Dictionary<string, decimal> {
					{"T_UkVisitors", 30}, //-api
					{"T_Registration", 5},
					{"T_Application", 3},
					{"T_Approved", 1},
					{"T_LoansOut", 10000},
					{"T_Repayments", 12000},

					{"M_UkVisitors", 500}, //-api
					{"M_Registration", 50},
					{"M_Application", 10},
					{"M_Approved", 5},
					{"M_LoansOut", 283600},
					{"M_Repayments", 55000},
					{"M_AvgLoanSize", 5000},
					{"M_AvgDailyLoans", 57720},
					{"M_AvgInterest", 0.0423M},

					//{"G_AvgDailyLoans", 5000},//
					//{"G_DefaultRate", 0.038M},//
					{"G_TotalLoans", 12500000}, //
					{"G_BookSize", 5600000}, //
					{"G_AvgInterest", 0.023M}, //
					//{"G_AvgLoanSize", 10000},//
					//{"G_AvgNewLoan", 12000}//
				}
			};

			return model;
		}

	}
}
