namespace EzTvDashboard.Models
{
	using System.Collections.Generic;

	public class DashboardModel
	{
		public Dictionary<string, decimal> Stats { get; set; }

		//not in use
		public List<MonthlyBoardModel> MonthlyBoard { get; set; }
		public List<MonthlyTrafficModel> MonthlyTraffic { get; set; }
		public List<MonthlyCollectionModel> MonthlyCollection { get; set; }
		public List<CollectionStatusModel> CollectionStatus { get; set; }
		public List<FunnelNumbersModel> FunnelNumbers { get; set; }
		public List<ExistingCustomersModel> ExistingCustomers { get; set; }

	}

	public class MonthlyBoardModel
	{
		public string Caption { get; set; }
		public int Count { get; set; }
		public string Value { get; set; }
		public decimal Benchmark { get; set; }
	}

	public class MonthlyTrafficModel
	{
		public string Caption { get; set; }
		public int Visits { get; set; }
		public int Applications { get; set; }
		public int Approvals { get; set; }
		public decimal Loans { get; set; }
	}

	public class MonthlyCollectionModel
	{
		public string Caption { get; set; }
		public int Count { get; set; }
		public decimal Value { get; set; }
	}

	public class CollectionStatusModel
	{
		public string Caption { get; set; }
		public int Clients { get; set; }
		public decimal Value { get; set; }
		public decimal Percent { get; set; }
	}

	public class FunnelNumbersModel
	{
		public string Caption { get; set; }
		public int Count { get; set; }
		public decimal Percent { get; set; }
	}

	public class ExistingCustomersModel
	{
		public string Caption { get; set; }
		public int Count { get; set; }
		public decimal Potential { get; set; }
	}
}