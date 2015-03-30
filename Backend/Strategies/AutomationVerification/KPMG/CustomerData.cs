namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System.Collections.Generic;

	internal class CustomerData {
		public CustomerData(SpLoadCashRequestsForAutomationReport.ResultRow sr, bool isDefault) {
			Data = new List<Datum>();

			Add(sr, isDefault);
		} // constructor

		public void Add(SpLoadCashRequestsForAutomationReport.ResultRow sr, bool isDefault) {} // Add

		public List<Datum> Data { get; private set; }
	} // class CustomerData
} // namespace
