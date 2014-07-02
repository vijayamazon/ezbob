namespace EzTvDashboard.Code
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Database;
	using GoogleAnalyticsLib;
	using Models;

	public class DashboardModelBuilder
	{
		private AConnection Db { get; set; }
		private static string GaCertThumb { get; set; }
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(DashboardModelBuilder));
		private static DashboardModel DashboardModel { get; set; }

		private static int CustomerId { get; set; }
		private static int CashRequestsId { get; set; }
		private static int LoanId { get; set; }
		private static int BrokerId { get; set; }
		public static DateTime LastChanged { get; set; }

		public DashboardModelBuilder()
		{
			Db = new SqlConnection();
			GaCertThumb = System.Configuration.ConfigurationManager.AppSettings["gaCertThumb"];
			DashboardModel = DashboardModel ?? BuildModel();
		}

		public bool SomethingChanged()
		{
			var changed = Db.ExecuteReader("EzTvIsChanged");
			bool isCustomerChanged = false;
			bool isCashRequestChanged = false;
			bool isLoanChanged = false;
			bool isBrokerChanged = false;
			foreach (DataRow row in changed.Rows)
			{
				int val;
				if (row["Table"].ToString() == "Security_User")
				{
					val = int.Parse(row["Val"].ToString());
					isCustomerChanged = val > CustomerId;
					CustomerId = val;
				}
				if (row["Table"].ToString() == "CashRequests")
				{
					val = int.Parse(row["Val"].ToString());
					isCashRequestChanged = val > CashRequestsId;
					CashRequestsId = val;
				}
				if (row["Table"].ToString() == "Loan")
				{
					val = int.Parse(row["Val"].ToString());
					isLoanChanged = val > LoanId;
					LoanId = val;
				}
				if (row["Table"].ToString() == "Broker")
				{
					val = int.Parse(row["Val"].ToString());
					isBrokerChanged = val > BrokerId;
					BrokerId = val;
				}
			}

			bool somethingChanged = isCustomerChanged || isCashRequestChanged || isLoanChanged || isBrokerChanged;
			if (somethingChanged)
			{
				Log.Debug("Something changed, building model");
				DashboardModel = BuildModel();
			}

			return true;
		}

		public DashboardModel GetModel()
		{
			return DashboardModel;
		}

		private DashboardModel BuildModel()
		{
			LastChanged = DateTime.UtcNow;
			var model = new DashboardModel
				{
					CollectionStatus = new List<CollectionStatusModel>(),
					ExistingCustomers = new List<ExistingCustomersModel>(),
					FunnelNumbers = new List<FunnelNumbersModel>(),
					MonthlyBoard = new List<MonthlyBoardModel>(),
					MonthlyCollection = new List<MonthlyCollectionModel>(),
					MonthlyTraffic = new List<MonthlyTrafficModel>(),
					Stats = new Dictionary<string, decimal>(),
					
				};

			var firstOfMonth = new DateTime(LastChanged.Year, LastChanged.Month, 1);
			Db.ForEachRow(
				(oReader, bRowsetStart) =>
				{
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

			try
			{
				var ga = new GoogleAnalytics();
				ga.Init(DateTime.UtcNow, GaCertThumb);
				var todayAnalytics = ga.FetchByCountry(DateTime.Today, LastChanged);
				var monthToDateAnalytics = ga.FetchByCountry(firstOfMonth, LastChanged);
				if (todayAnalytics.ContainsKey("United Kingdom"))
				{
					model.Stats["T_UkVisitors"] = todayAnalytics["United Kingdom"].Users;
				}

				if (monthToDateAnalytics.ContainsKey("United Kingdom"))
				{
					model.Stats["M_UkVisitors"] = monthToDateAnalytics["United Kingdom"].Users;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorFormat("Failed to load google analytics values \n{0}", ex);
			}

			return model;
		}

		public DashboardModel BuildFakeModel()
		{
			LastChanged = DateTime.UtcNow;
			var model = new DashboardModel
				{
					
					Stats = new Dictionary<string, decimal>
						{
							{"T_UkVisitors", 30},//-api
							{"T_Registration", 5},
							{"T_Application", 3},
							{"T_Approved", 1},
							{"T_LoansOut", 10000},
							{"T_Repayments", 12000},

							{"M_UkVisitors", 500},//-api
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
							{"G_TotalLoans", 12500000},//
							{"G_BookSize", 5600000},//
							{"G_AvgInterest", 0.023M},//
							//{"G_AvgLoanSize", 10000},//
							//{"G_AvgNewLoan", 12000}//
						},
					MonthlyBoard = new List<MonthlyBoardModel>
						{
							new MonthlyBoardModel
								{
									Caption = "Total loans",
									Count = 50,
									Value = "850K",
									Benchmark = -0.1M
								},
							new MonthlyBoardModel
								{
									Caption = "New loans",
									Count = 30,
									Value = "350K",
									Benchmark = 0.2M
								},
							new MonthlyBoardModel
								{
									Caption = "Repeating loans",
									Count = 20,
									Value = "500K",
									Benchmark = -0.2M
								},
							new MonthlyBoardModel
								{
									Caption = "Portfolio change",
									Count = 12,
									Value = "150K",
									Benchmark = 0.02M
								},
							new MonthlyBoardModel
								{
									Caption = "Planned repaid this month",
									Count = 50,
									Value = "400K",
									Benchmark = 0.03M
								},
							new MonthlyBoardModel
								{
									Caption = "Average rate",
									Count = 0,
									Value = "3.5%",
									Benchmark = -0.03M
								},
							new MonthlyBoardModel
								{
									Caption = "Total monthly income",
									Count = 0,
									Value = "1.5M",
									Benchmark = 0.05M
								},
						},
					MonthlyTraffic = new List<MonthlyTrafficModel>
						{
							new MonthlyTrafficModel
								{
									Caption = "Total new UK uniques",
									Visits = 5500,
									Applications = 275,
									Approvals = 138,
									Loans = 46
								},
							new MonthlyTrafficModel
								{
									Caption = "PPC",
									Visits = 1500,
									Applications = 75,
									Approvals = 38,
									Loans = 13
								},
							new MonthlyTrafficModel
								{
									Caption = "Organic",
									Visits = 1500,
									Applications = 60,
									Approvals = 32,
									Loans = 46
								},
							new MonthlyTrafficModel
								{
									Caption = "Ebay",
									Visits = 1200,
									Applications = 50,
									Approvals = 13,
									Loans = 5
								},
							new MonthlyTrafficModel
								{
									Caption = "Brokers",
									Visits = 200,
									Applications = 40,
									Approvals = 138,
									Loans = 3
								},
							new MonthlyTrafficModel
								{
									Caption = "Grow the money",
									Visits = 200,
									Applications = 40,
									Approvals = 138,
									Loans = 3
								},
							new MonthlyTrafficModel
								{
									Caption = "rest",
									Visits = 400,
									Applications = 20,
									Approvals = 10,
									Loans = 2
								},
						},
					MonthlyCollection = new List<MonthlyCollectionModel>
						{
							new MonthlyCollectionModel
								{
									Caption = "New late customers",
									Count = 10,
									Value = 35000
								},
							new MonthlyCollectionModel
								{
									Caption = "Recovey",
									Count = 5,
									Value = 15000
								},
							new MonthlyCollectionModel
								{
									Caption = "Passed to CCI",
									Count = 3,
									Value = 10000
								},
						},
					CollectionStatus = new List<CollectionStatusModel>
						{
							new CollectionStatusModel
								{
									Caption = "Write off",
									Clients = 20,
									Value = 250000,
									Percent = 0.02M
								},
							new CollectionStatusModel
								{
									Caption = "Default",
									Clients = 30,
									Value = 200000,
									Percent = 0.023M
								},
							new CollectionStatusModel
								{
									Caption = "Legal",
									Clients = 10,
									Value = 125000,
									Percent = 0.013M
								},
							new CollectionStatusModel
								{
									Caption = "Risky & Late",
									Clients = 12,
									Value = 80000,
									Percent = 0.012M
								},
							new CollectionStatusModel
								{
									Caption = "Total portfolio",
									Clients = 1000,
									Value = 120000000,
									Percent = 1
								},
						},
					FunnelNumbers = new List<FunnelNumbersModel>
						{
							new FunnelNumbersModel
								{
									Caption = "Visits",
									Count = 20000,
									Percent = 1
								},
							new FunnelNumbersModel
								{
									Caption = "start wizard",
									Count = 1000,
									Percent = 0.05M,
								},
							new FunnelNumbersModel
								{
									Caption = "step 1",
									Count = 800,
									Percent = 0.04M
								},
							new FunnelNumbersModel
								{
									Caption = "step 2",
									Count = 700,
									Percent = 0.035M
								},
							new FunnelNumbersModel
								{
									Caption = "step 3",
									Count = 600,
									Percent = 0.024M
								},
							new FunnelNumbersModel
								{
									Caption = "finished",
									Count = 500,
									Percent = 0.0086M
								},
							new FunnelNumbersModel
								{
									Caption = "approved",
									Count = 150,
									Percent = 0.008M
								},
							new FunnelNumbersModel
								{
									Caption = "loan",
									Count = 120,
									Percent = 0.006M
								}
						},
					ExistingCustomers = new List<ExistingCustomersModel>
						{
							new ExistingCustomersModel
								{
									Caption = "Active Loan",
									Count = 1200,
									Potential = 5350000
								},
							new ExistingCustomersModel
								{
									Caption = "No Loan",
									Count = 300,
									Potential = 600000
								},
							new ExistingCustomersModel
								{
									Caption = "Approved never took",
									Count = 200,
									Potential = 800000
								},
							new ExistingCustomersModel
								{
									Caption = "Had one - not now",
									Count = 100,
									Potential = 200000
								},
						}
				};

			return model;
		}

	}


}