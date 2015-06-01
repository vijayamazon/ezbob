namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport {
	using System.Collections.Generic;

	internal class CustomerDecisions {
		public CustomerDecisions(int customerID, bool isAlibaba) {
			Manual = new ManualInterface(this);
			Auto = new AutoInterface(this);

			ManualDecisions = new List<ManualDecision>();
			AutoDecisions = new List<AutoDecision>();

			CustomerID = customerID;
			IsAlibaba = isAlibaba;
		} // constructor

		public int CustomerID { get; private set; }

		public bool IsAlibaba { get; private set; }

		public List<ManualDecision> ManualDecisions { get; private set; }
		public ManualInterface Manual { get; private set; }

		public List<AutoDecision> AutoDecisions { get; set; }
		public AutoInterface Auto { get; private set; }

		public AutoDecision CurrentAutoDecision { get; set; }

		public class ManualInterface {
			public ManualInterface(CustomerDecisions cd) {
				this.cd = cd;
			} // constructor

			public ManualDecision First { get { return this.cd.ManualDecisions[0]; } }

			public ManualDecision Last { get { return this.cd.ManualDecisions[this.cd.ManualDecisions.Count - 1]; } }

			private readonly CustomerDecisions cd;
		} // ManualInterface

		public class AutoInterface {
			public AutoInterface(CustomerDecisions cd) {
				this.cd = cd;
			} // constructor

			public AutoDecision First { get { return this.cd.AutoDecisions[0]; } }

			public AutoDecision Current { get { return this.cd.CurrentAutoDecision; } }

			public AutoDecision Last { get { return this.cd.AutoDecisions[this.cd.AutoDecisions.Count - 1]; } }

			private readonly CustomerDecisions cd;
		} // AutoInterface
	} // class CustomerDecisions
} // namespace
