namespace Ezbob.Backend.Strategies.SalesForce {
	using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;

	public class UpdateOpportunity : AStrategy {

		public UpdateOpportunity(int customerID, OpportunityModel model) {
			salesForce = ObjectFactory
				.With("userName").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceUserName.Value)
				.With("password").EqualTo(ConfigManager.CurrentValues.Instance.SalesForcePassword.Value)
				.With("token").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceToken.Value)
				.With("environment").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceEnvironment.Value)
				.GetInstance<ISalesForceAppClient>();
			this.customerID = customerID;
			this.opportunityModel = model;
		}
		public override string Name { get { return "UpdateOpportunity"; } }

		public override void Execute() {
			Log.Info("Updating SalesForce opportunity to customer {0} ", customerID);
			salesForce.UpdateOpportunity(opportunityModel);
		}
		private readonly ISalesForceAppClient salesForce;
		private readonly int customerID;
		private readonly OpportunityModel opportunityModel;
	}
}
