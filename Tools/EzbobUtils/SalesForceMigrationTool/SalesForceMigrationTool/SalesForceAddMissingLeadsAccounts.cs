namespace SalesForceMigrationTool {
	using Ezbob.Database;
	using log4net;
	using SalesForceLib;
	using SalesForceLib.Models;

	public class SalesForceAddMissingLeadsAccounts {
		private readonly ISalesForceAppClient salesForce;
		private readonly AConnection DB;
		private const int MaxLeadSourceLength = 30;
		protected static ILog Log = LogManager.GetLogger(typeof(SalesForceAddMissingLeadsAccounts));

		public SalesForceAddMissingLeadsAccounts(ISalesForceAppClient client, AConnection db) {
			this.salesForce = client;
			this.DB = db;
		}

		public void AddLead(int customerID) {

			LeadAccountModel model = this.DB.FillFirst<LeadAccountModel>(
				"SF_LoadAccountLead",
				CommandSpecies.StoredProcedure,
				new QueryParameter("Email", null),
				new QueryParameter("CustomerID", customerID),
				new QueryParameter("IsBrokerLead", false),
				new QueryParameter("IsVipLead", false));


			if (string.IsNullOrEmpty(model.CompanyName)) {
				model.CompanyName = model.Name;
			}

			if (string.IsNullOrEmpty(model.CompanyName)) {
				model.CompanyName = "No name";
			}

			if (!string.IsNullOrEmpty(model.LeadSource) && model.LeadSource.Length > MaxLeadSourceLength) {
				model.LeadSource = model.LeadSource.Substring(0, MaxLeadSourceLength);
			}

			model.Email = model.Email.ToLower();

			this.salesForce.CreateUpdateLeadAccount(model);

			if (this.salesForce.HasError) {
				Log.ErrorFormat("Failed to create lead {0}, error {1}", customerID, this.salesForce.Error);
			}
		}

		public static int[] missingCustomers = {};
	}
}
