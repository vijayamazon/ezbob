namespace Ezbob.Backend.Strategies.SalesForce {
	using Ezbob.Database;
	using SalesForceLib;
	using SalesForceLib.Models;

	public class AddOpportunity : AStrategy {

		public AddOpportunity(int customerID) {
			salesForce = new ApiClient();
			this.customerID = customerID;
		}
		public override string Name { get { return "AddOpportunity"; } }

		public override void Execute() {
			salesForce.CreateUpdateOpportunity(new OpportunityModel());
		}
		private readonly ApiClient salesForce;
		private readonly int customerID;
	}
}
