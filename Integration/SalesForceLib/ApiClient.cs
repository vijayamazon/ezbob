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

		public void CreateUpdateOpportunity(OpportunityModel model) {
			Log.DebugFormat("CreateUpdateOpportunity\n {0}", model.ToStringExtension());
			ApiResponse response = api.CreateUpdateOpportunity(model);
			if (!response.Success) {
				Log.ErrorFormat("SalesForce CreateUpdateOpportunity failed for customer {0}, error: {1}", model.Email, response.Error);
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
