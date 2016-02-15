namespace SalesForceRestApiTestClient {
	public class CreateBrokerRequest {
		public int? BrokerID { get; set; }
		public string Origin { get; set; }
		public string ContactEmail { get; set; }

		public string FirmName { get; set; }
		public string FirmRegNum { get; set; }
		public string ContactName { get; set; }
		public string ContactMobile { get; set; }
		public string ContactOtherPhone { get; set; }
		public string SourceRef { get; set; }
		public decimal? EstimatedMonthlyClientAmount { get; set; }
		public string FirmWebSiteUrl { get; set; }
		public int? EstimatedMonthlyApplicationCount { get; set; }
		public bool IsTest { get; set; }
		public string LicenseNumber { get; set; }
		public bool FCARegistered { get; set; }
	}
}
