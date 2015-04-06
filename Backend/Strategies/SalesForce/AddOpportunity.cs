namespace Ezbob.Backend.Strategies.SalesForce {
	using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;

	public class AddOpportunity : AStrategy {

		public AddOpportunity(int customerID, OpportunityModel model) {
            this.salesForce = ObjectFactory
				.With("userName").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceUserName.Value)
				.With("password").EqualTo(ConfigManager.CurrentValues.Instance.SalesForcePassword.Value)
				.With("token").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceToken.Value)
				.With("environment").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceEnvironment.Value)
				.GetInstance<ISalesForceAppClient>();
			this.customerID = customerID;
			this.opportunityModel = model;
		}
		public override string Name { get { return "AddOpportunity"; } }

		public override void Execute() {
            Log.Info("Adding SalesForce opportunity to customer {0} ", this.customerID);
		    
            this.opportunityModel.Email = this.opportunityModel.Email.ToLower();

            this.salesForce.CreateOpportunity(this.opportunityModel);
		}
		private readonly ISalesForceAppClient salesForce;
		private readonly int customerID;
		private readonly OpportunityModel opportunityModel;
	}
}
