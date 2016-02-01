namespace EzTvDashboard.Code {
	using System;
	using System.Collections.Generic;
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

			var changed = Db.ExecuteEnumerable("EzTvIsChanged", CommandSpecies.StoredProcedure);

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
				return true;
			}

			return false;
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

				Tuple<int, int> ezbobVisitors = GetAnalyticsData(DbConsts.EzbobProfileID, firstOfMonth);
				Tuple<int, int> everlineVisitors = GetAnalyticsData(DbConsts.EverlineProfileID, firstOfMonth);

				model.Stats["T_UkVisitors"] = ezbobVisitors.Item1 + everlineVisitors.Item1;
				model.Stats["M_UkVisitors"] = ezbobVisitors.Item2 + everlineVisitors.Item2;

				model.Stats["T_UkVisitorsEverline"] = everlineVisitors.Item1;
				model.Stats["M_UkVisitorsEverline"] = everlineVisitors.Item2;
			} catch (Exception ex) {
				Log.ErrorFormat("Failed to load google analytics values \n{0}", ex);
			}

			return model;
		}

		/// <returns>Item1 = todayVisitors, Item2 = monthVisitors</returns>
		private Tuple<int, int> GetAnalyticsData(string profileID, DateTime firstOfMonth)
		{
			int todayVisitors = 0;
			int monthVisitors = 0;
			var ga = new GoogleAnalytics();
			ga.Init(DateTime.UtcNow, GaCertThumb, profileID);
			var todayAnalytics = ga.FetchByCountry(DateTime.Today, LastChanged);
			var monthToDateAnalytics = ga.FetchByCountry(firstOfMonth, LastChanged);
			if (todayAnalytics.ContainsKey("United Kingdom")) {
				todayVisitors = todayAnalytics["United Kingdom"].Users;
			}

			if (monthToDateAnalytics.ContainsKey("United Kingdom")) {
				monthVisitors = monthToDateAnalytics["United Kingdom"].Users;
			}

			return new Tuple<int, int>(todayVisitors, monthVisitors);
		}

		public DashboardModel BuildFakeModel() {
			LastChanged = DateTime.UtcNow;
			var model = new DashboardModel {
				Stats = new Dictionary<string, decimal> {
					{"T_UkVisitors", 30}, //-api
					{"T_Registration", 5},
					{"T_Application", 3},
					{"T_Approved", 2},
					{"T_LoansOut", 10000},
					{"T_Repayments", 12000},

					{"M_UkVisitors", 500}, //-api
					{"M_Registration", 50},
					{"M_Application", 10},
					{"M_Approved", 5},
					{"M_LoansOut", 283600},
					{"M_Repayments", 55000},

					{"T_UkVisitorsEverline", 20}, //-api
					{"T_RegistrationEverline", 4},
					{"T_ApplicationEverline", 1},
					{"T_ApprovedEverline", 1},
					{"T_LoansOutEverline", 4000},
					{"T_RepaymentsEverline", 5000},

					{"M_UkVisitorsEverline", 400}, //-api
					{"M_RegistrationEverline", 10},
					{"M_ApplicationEverline", 4},
					{"M_ApprovedEverline", 3},
					{"M_LoansOutEverline", 183600},
					{"M_RepaymentsEverline", 15000},

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
