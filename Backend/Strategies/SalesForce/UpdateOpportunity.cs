namespace Ezbob.Backend.Strategies.SalesForce {
	using SalesForceLib;
	using SalesForceLib.Models;

	public class UpdateOpportunity : AStrategy {

		public UpdateOpportunity(int customerID, OpportunityModel model) {
			salesForce = new ApiClient();
			this.customerID = customerID;
			this.opportunityModel = model;
		}
		public override string Name { get { return "UpdateOpportunity"; } }

		public override void Execute() {
			Log.Info("Updating SalesForce opportunity to customer {0} ", customerID);
			salesForce.UpdateOpportunity(opportunityModel);
		}
		private readonly ApiClient salesForce;
		private readonly int customerID;
		private readonly OpportunityModel opportunityModel;
	}
}
