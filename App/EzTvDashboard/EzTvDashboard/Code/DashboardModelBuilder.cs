namespace EzTvDashboard.Code
{
	using System;
	using System.Collections.Generic;
	using Models;

	public class DashboardModelBuilder
	{
		public DashboardModel BuildModel()
		{
			var model = new DashboardModel();
			//todo implement
			return model;
		}

		public DashboardModel BuildFakeModel()
		{
			var model = new DashboardModel();
			model.AvgDailyLoans = 55000;
			model.DefaultRate = 0.038M;
			model.TotalLoans = 12500000;
			model.BookSize = 5600000;
			model.AvgInterest = 0.039M;
			model.AvgLoanSize = 10000;
			model.AvgNewLoan = 12000;
			model.OpenBugs = 3;
			model.TodayLoans = 65000;

			model.MonthlyBoard = new List<MonthlyBoardModel>
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
				};

			model.MonthlyTraffic = new List<MonthlyTrafficModel>
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
				};

			model.MonthlyCollection = new List<MonthlyCollectionModel>
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
				};

			model.CollectionStatus = new List<CollectionStatusModel>
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
				};

			model.FunnelNumbers = new List<FunnelNumbersModel>
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
				};

			model.ExistingCustomers = new List<ExistingCustomersModel>
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
				};
			return model;
		}

		public bool SomethingChanged()
		{
			var r = new Random();
			var a = r.Next(0, 1);
			return a != 0;
		}
	}
}