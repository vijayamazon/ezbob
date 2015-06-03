namespace SalesForceLib
{
	using log4net;
	using SalesForceLib.Models;

	public class FakeApiClient : ISalesForceAppClient
    {
        public string Error { get; set; }

	    public bool HasError {
	        get {
	            return false;
	        }
	    }

	    public string Model { get; set; }

		public FakeApiClient(string userName, string password, string token, string environment) {
			api = new Api();
		}

		public void CreateUpdateLeadAccount(LeadAccountModel model) {
			Log.InfoFormat("SalesForce CreateUpdateLeadAccount\n {0}", model.ToStringExtension());
			ApiResponse response = api.CreateUpdateLeadAccount(model);
			if (!response.IsSuccess) {
				Log.ErrorFormat("SalesForce CreateUpdateLeadAccount failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void CreateOpportunity(OpportunityModel model) {
			Log.InfoFormat("SalesForce CreateOpportunity\n {0}", model.ToStringExtension());
			ApiResponse response = api.CreateOpportunity(model);
			if (!response.IsSuccess) {
				Log.ErrorFormat("SalesForce CreateOpportunity failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void UpdateOpportunity(OpportunityModel model) {
			Log.InfoFormat("SalesForce UpdateOpportunity\n {0}", model.ToStringExtension());
			ApiResponse response = api.UpdateOpportunity(model);
			if (!response.IsSuccess) {
				Log.ErrorFormat("SalesForce UpdateOpportunity failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void CreateUpdateContact(ContactModel model) {
			Log.InfoFormat("SalesForce CreateUpdateContact\n {0}", model.ToStringExtension());
			ApiResponse response = api.CreateUpdateContact(model);
			if (!response.IsSuccess) {
				Log.ErrorFormat("SalesForce ContactModel failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void CreateTask(TaskModel model) {
			Log.InfoFormat("SalesForce CreateTask\n {0}", model.ToStringExtension());
			ApiResponse response = api.CreateTask(model);
			if (!response.IsSuccess) {
				Log.ErrorFormat("SalesForce CreateTask failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void CreateActivity(ActivityModel model) {
			Log.InfoFormat("SalesForce CreateActivity\n {0}", model.ToStringExtension());
			ApiResponse response = api.CreateActivity(model);
			if (!response.IsSuccess) {
				Log.ErrorFormat("SalesForce CreateActivity failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void ChangeEmail(string currentEmail, string newEmail) {
			Log.InfoFormat("SalesForce ChangeEmail from {0} to {1}", currentEmail, newEmail);
			ApiResponse response = api.ChangeEmail(currentEmail, newEmail);
			if (!response.IsSuccess) {
				Log.ErrorFormat("SalesForce ChangeEmail failed from {0} to {1} failed, error: {2}", currentEmail, newEmail, response.Error);
			}
		}

		public GetActivityResultModel GetActivity(string email) {
			Log.InfoFormat("SalesForce GetActivity for {0}", email);
			ApiResponse response = api.GetActivity(email);
			return null;
		}

		private readonly Api api;
		protected static readonly ILog Log = LogManager.GetLogger(typeof (FakeApiClient));
	}
}
