namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport {
	using System.Collections.Generic;

	internal class CustomerDecisions {
		public CustomerDecisions(int customerID, bool isAlibaba) {
			ManualDecisions = new List<ManualDecision>();
			CustomerID = customerID;
			IsAlibaba = isAlibaba;
		} // constructor

		public int CustomerID { get; private set; }

		public bool IsAlibaba { get; private set; }

		public List<ManualDecision> ManualDecisions { get; private set; }

		public AutoDecision AutoDecision { get; set; }
	} // class CustomerDecisions
} // namespace
