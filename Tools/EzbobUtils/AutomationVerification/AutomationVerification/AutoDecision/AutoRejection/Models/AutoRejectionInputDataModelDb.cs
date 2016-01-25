namespace AutomationCalculator.AutoDecision.AutoRejection.Models {
	using System;

	public class AutoRejectionInputDataModelDb {
		//from sp
		public string CustomerStatus { get; set; }
		public int ExperianScore { get; set; }
		public int CompanyScore { get; set; }
		public bool WasApproved { get; set; }
		public bool IsBrokerClient { get; set; }
		public bool HasErrorMp { get; set; }
		public bool HasCompanyFiles { get; set; }
		public DateTime? IncorporationDate { get; set; }
		public DateTime? ConsumerDataTime { get; set; }
	} // class AutoRejectionInputDataModelDb
} // namespace