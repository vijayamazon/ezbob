namespace Ezbob.Backend.Strategies.SalesForce {
	using System;
	using Ezbob.Database;
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
		} // constructor

		public override string Name { get { return "AddOpportunity"; } }

		public override void Execute() {
			Log.Info("Adding SalesForce opportunity to customer {0} ", this.customerID);

			this.opportunityModel.Email = this.opportunityModel.Email.ToLower();

			SalesForceRetier.Execute(
				ConfigManager.CurrentValues.Instance.SalesForceNumberOfRetries,
				ConfigManager.CurrentValues.Instance.SalesForceRetryWaitSeconds,
				this.salesForce,
				() => { this.salesForce.CreateOpportunity(this.opportunityModel); }
			);

			if (this.salesForce.HasError) {
				DB.ExecuteNonQuery("SalesForceSaveError", CommandSpecies.StoredProcedure,
					new QueryParameter("Now", DateTime.UtcNow),
					new QueryParameter("CustomerID", this.customerID),
					new QueryParameter("Type", Name),
					new QueryParameter("Model", this.salesForce.Model),
					new QueryParameter("Error", this.salesForce.Error)
				);
			} // if
		} // Execute

		private readonly ISalesForceAppClient salesForce;
		private readonly int customerID;
		private readonly OpportunityModel opportunityModel;
	} // class AddOpportunity
} // namespace
