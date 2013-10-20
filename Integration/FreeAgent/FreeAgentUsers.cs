namespace FreeAgent
{
	using System;
	using System.Collections.Generic;

	[Serializable]
	public class FreeAgentUsers
	{
		public string url { get; set; }
		public string first_name { get; set; }
		public string last_name { get; set; }
		public string email { get; set; }
		public string role { get; set; }
		public int permission_level { get; set; }
		public decimal opening_mileage { get; set; }
		public DateTime? updated_at { get; set; }
		public DateTime? created_at { get; set; }
	}

	[Serializable]
	public class FreeAgentUsersList
	{
		public List<FreeAgentUsers> Users { get; set; }
	}
}