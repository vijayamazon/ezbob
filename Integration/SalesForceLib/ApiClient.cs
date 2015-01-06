namespace SalesForceLib
{
	using log4net;
	using SalesForceLib.Models;

	public class ApiClient
    {
		public ApiClient() {
			api = new Api(); //todo replace with real service client
		}

		public void CreateUpdateLeadAccount(LeadAccountModel model) {
			Log.DebugFormat("CreateUpdateLeadAccount\n {0}", model.ToStringExtension());
			ApiResponse response = api.CreateUpdateLeadAccount(model);
			if (!response.Success) {
				Log.ErrorFormat("SalesForce CreateUpdateLeadAccount failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void CreateOpportunity(OpportunityModel model) {
			Log.DebugFormat("CreateOpportunity\n {0}", model.ToStringExtension());
			ApiResponse response = api.CreateOpportunity(model);
			if (!response.Success) {
				Log.ErrorFormat("SalesForce CreateOpportunity failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void UpdateOpportunity(OpportunityModel model) {
			Log.DebugFormat("UpdateOpportunity\n {0}", model.ToStringExtension());
			ApiResponse response = api.UpdateOpportunity(model);
			if (!response.Success) {
				Log.ErrorFormat("SalesForce UpdateOpportunity failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void CreateUpdateContact(ContactModel model) {
			Log.DebugFormat("CreateUpdateContact\n {0}", model.ToStringExtension());
			ApiResponse response = api.CreateUpdateContact(model);
			if (!response.Success) {
				Log.ErrorFormat("SalesForce ContactModel failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void CreateTask(TaskModel model) {
			Log.DebugFormat("CreateTask\n {0}", model.ToStringExtension());
			ApiResponse response = api.CreateTask(model);
			if (!response.Success) {
				Log.ErrorFormat("SalesForce CreateTask failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void CreateEvent(EventModel model) {
			Log.DebugFormat("CreateEvent\n {0}", model.ToStringExtension());
			ApiResponse response = api.CreateEvent(model);
			if (!response.Success) {
				Log.ErrorFormat("SalesForce CreateEvent failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}


		private readonly Api api;
		private readonly ILog Log = LogManager.GetLogger(typeof (ApiClient));
	}
}
