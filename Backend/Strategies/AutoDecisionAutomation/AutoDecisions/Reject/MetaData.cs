namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject {
	using System;
	using System.Collections.Generic;

	public class MetaData {
		public MetaData() {
			ValidationErrors = new List<string>();
		} // constructor

		public string RowType { get; set; }
		public int ApprovedCrID { get; set; }
		public int BrokerID { get; set; }
		public int FraudStatusID { get; set; }
		public string CustomerStatusName { get; set; }
		public string CustomerStatusEnabled { get; set; }
		public int ConsumerScore { get; set; }
		public int BusinessScore { get; set; }
		public DateTime? IncorporationDate { get; set; }
		public int CompanyFilesCount { get; set; }
		public bool IsLtd { get; set; }
		public string CompanyRefNum { get; set; }
		public DateTime? ConsumerDataTime { get; set; }
		public long ConsumerServiceLogID { get; set; }
		public long CompanyServiceLogID { get; set; }

		public List<string> ValidationErrors { get; private set; }

		public void Validate() {
			if (string.IsNullOrWhiteSpace(RowType))
				throw new Exception("Meta data was not loaded.");
		} // Validate
	} // class MetaData
} // namespace
