namespace Ezbob.Backend.Strategies.SalesForce {
	using SalesForceLib;
	using SalesForceLib.Models;

	public class AddOpportunity : AStrategy {

		public AddOpportunity(int customerID, OpportunityModel model) {
			salesForce = new ApiClient();
			this.customerID = customerID;
			this.opportunityModel = model;
		}
		public override string Name { get { return "AddOpportunity"; } }

		public override void Execute() {
			Log.Info("Adding SalesForce opportunity to customer {0} ", customerID);
			salesForce.CreateOpportunity(opportunityModel);
		}
		private readonly ApiClient salesForce;
		private readonly int customerID;
		private readonly OpportunityModel opportunityModel;
	}
}
