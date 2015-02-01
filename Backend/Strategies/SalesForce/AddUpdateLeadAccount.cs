namespace Ezbob.Backend.Strategies.SalesForce {
	using Ezbob.Database;
	using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;

	public class AddUpdateLeadAccount : AStrategy {
		public AddUpdateLeadAccount(string email, int? customerID, bool isBrokerLead, bool isVipLead) {
			salesForce = ObjectFactory.GetInstance<ISalesForceAppClient>();
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
				new QueryParameter("Email", email),
				new QueryParameter("CustomerID", customerID),
				new QueryParameter("IsBrokerLead", isBrokerLead),
				new QueryParameter("IsVipLead", isVipLead));

			salesForce.CreateUpdateLeadAccount(model);
		}

		private readonly ISalesForceAppClient salesForce;
		private readonly string email;
		private readonly int? customerID;
		private readonly bool isBrokerLead;
		private readonly bool isVipLead;
	}
}
