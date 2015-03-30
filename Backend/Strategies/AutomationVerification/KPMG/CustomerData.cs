namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System.Collections.Generic;

	internal class CustomerData {
		public CustomerData(SpLoadCashRequestsForAutomationReport.ResultRow sr) {
			Data = new List<Datum>();

			Add(sr);
		} // constructor

		public void Add(SpLoadCashRequestsForAutomationReport.ResultRow sr) {} // Add

		public List<Datum> Data { get; private set; }
	} // class CustomerData
} // namespace
