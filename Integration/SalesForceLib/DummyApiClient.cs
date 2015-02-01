namespace SalesForceLib
{
	using log4net;
	using SalesForceLib.Models;

	public class DummyApiClient : ISalesForceAppClient
    {
		public DummyApiClient() {
			api = new Api();
		}

		public void CreateUpdateLeadAccount(LeadAccountModel model) {
			Log.InfoFormat("SalesForce CreateUpdateLeadAccount\n {0}", model.ToStringExtension());
			ApiResponse response = api.CreateUpdateLeadAccount(model);
			if (!response.Success) {
				Log.ErrorFormat("SalesForce CreateUpdateLeadAccount failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void CreateOpportunity(OpportunityModel model) {
			Log.InfoFormat("SalesForce CreateOpportunity\n {0}", model.ToStringExtension());
			ApiResponse response = api.CreateOpportunity(model);
			if (!response.Success) {
				Log.ErrorFormat("SalesForce CreateOpportunity failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void UpdateOpportunity(OpportunityModel model) {
			Log.InfoFormat("SalesForce UpdateOpportunity\n {0}", model.ToStringExtension());
			ApiResponse response = api.UpdateOpportunity(model);
			if (!response.Success) {
				Log.ErrorFormat("SalesForce UpdateOpportunity failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void CreateUpdateContact(ContactModel model) {
			Log.InfoFormat("SalesForce CreateUpdateContact\n {0}", model.ToStringExtension());
			ApiResponse response = api.CreateUpdateContact(model);
			if (!response.Success) {
				Log.ErrorFormat("SalesForce ContactModel failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void CreateTask(TaskModel model) {
			Log.InfoFormat("SalesForce CreateTask\n {0}", model.ToStringExtension());
			ApiResponse response = api.CreateTask(model);
			if (!response.Success) {
				Log.ErrorFormat("SalesForce CreateTask failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void CreateActivity(ActivityModel model) {
			Log.InfoFormat("SalesForce CreateEvent\n {0}", model.ToStringExtension());
			ApiResponse response = api.CreateActivity(model);
			if (!response.Success) {
				Log.ErrorFormat("SalesForce CreateEvent failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void ChangeEmail(string currentEmail, string newEmail) {
			Log.InfoFormat("SalesForce ChangeEmail from {0} to {1}", currentEmail, newEmail);
			ApiResponse response = api.ChangeEmail(currentEmail, newEmail);
			if (!response.Success) {
				Log.ErrorFormat("SalesForce ChangeEmail failed from {0} to {1} failed, error: {2}", currentEmail, newEmail, response.Error);
			}
		}

		private readonly Api api;
		private readonly ILog Log = LogManager.GetLogger(typeof (DummyApiClient));
	}
}
