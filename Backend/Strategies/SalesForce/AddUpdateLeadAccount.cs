namespace Ezbob.Backend.Strategies.SalesForce {
    using System;
    using Ezbob.Database;
	using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;

	public class AddUpdateLeadAccount : AStrategy {
		public AddUpdateLeadAccount(string email, int? customerID, bool isBrokerLead, bool isVipLead) {
		    this.salesForce = ObjectFactory
				.With("userName").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceUserName.Value)
				.With("password").EqualTo(ConfigManager.CurrentValues.Instance.SalesForcePassword.Value)
				.With("token").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceToken.Value)
				.With("environment").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceEnvironment.Value)
				.GetInstance<ISalesForceAppClient>();
			this.email = email;
			this.customerID = customerID;
			this.isBrokerLead = isBrokerLead;
			this.isVipLead = isVipLead;
		}
		public override string Name { get { return "AddUpdateLeadAccount"; } }

		public override void Execute() {
			LeadAccountModel model = DB.FillFirst<LeadAccountModel>(
				"SF_LoadAccountLead",
				CommandSpecies.StoredProcedure,
				new QueryParameter("Email", this.email),
				new QueryParameter("CustomerID", this.customerID),
				new QueryParameter("IsBrokerLead", this.isBrokerLead),
				new QueryParameter("IsVipLead", this.isVipLead));


			if (string.IsNullOrEmpty(model.CompanyName)) {
				model.CompanyName = model.Name;
			}

			if (string.IsNullOrEmpty(model.CompanyName)) {
				model.CompanyName = "No name";
			}

            model.Email = model.Email.ToLower();

			this.salesForce.CreateUpdateLeadAccount(model);

            if (this.salesForce.HasError) {
                DB.ExecuteNonQuery("SalesForceSaveError", CommandSpecies.StoredProcedure,
                    new QueryParameter("Now", DateTime.UtcNow),
                    new QueryParameter("CustomerID", this.customerID),
                    new QueryParameter("Type", this.Name),
                    new QueryParameter("Model", this.salesForce.Model),
                    new QueryParameter("Error", this.salesForce.Error));
            }
		}

		private readonly ISalesForceAppClient salesForce;
		private readonly string email;
		private readonly int? customerID;
		private readonly bool isBrokerLead;
		private readonly bool isVipLead;
	}
}
