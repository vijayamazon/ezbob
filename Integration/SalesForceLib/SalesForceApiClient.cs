namespace SalesForceLib
{
	using log4net;
	using SalesForceLib.Models;
	using SalesForceLib.SalesForceServiceNS;

	public class SalesForceApiClient : ISalesForceAppClient
    {
		public SalesForceApiClient() {
			api = new EzbobWebServicesPortTypeClient("EzbobWebServicesBinding");
		}

		public void CreateUpdateLeadAccount(LeadAccountModel model) {
			Log.InfoFormat("SalesForce CreateUpdateLeadAccount\n {0}", model.ToStringExtension());
			string result;
			var response = api.LeadAccountService(new SessionHeader(), new CallOptions(), new DebuggingHeader(), new AllowFieldTruncationHeader(), model.ToJsonExtension(), out result);
			//if (!response.Success) {
			Log.ErrorFormat("SalesForce CreateUpdateLeadAccount failed for customer {0}, error: {1}", model.Email, result);
			//}

		}

		public void CreateOpportunity(OpportunityModel model) {
			Log.InfoFormat("SalesForce CreateOpportunity\n {0}", model.ToStringExtension());
			string result;
			var response = api.CreateOpportunityService(new SessionHeader(), new CallOptions(), new DebuggingHeader(), new AllowFieldTruncationHeader(), model.ToJsonExtension(), out result);
			//if (!response.Success) {
				Log.ErrorFormat("SalesForce CreateOpportunity failed for customer {0}, error: {1}", model.Email, result);
			//}
		}

		public void UpdateOpportunity(OpportunityModel model) {
			Log.InfoFormat("SalesForce UpdateOpportunity\n {0}", model.ToStringExtension());
			string result;
			var response = api.UpdateCloseOpportunityService(new SessionHeader(), new CallOptions(), new DebuggingHeader(), new AllowFieldTruncationHeader(), model.ToJsonExtension(), out result);
			//if (!response.Success) {
				Log.ErrorFormat("SalesForce UpdateOpportunity failed for customer {0}, error: {1}", model.Email, result);
			//}
		}

		public void CreateUpdateContact(ContactModel model) {
			Log.InfoFormat("SalesForce CreateUpdateContact\n {0}", model.ToStringExtension());
			string result;
			var response = api.ContactService(new SessionHeader(), new CallOptions(), new DebuggingHeader(), new AllowFieldTruncationHeader(), model.ToJsonExtension(), out result);
			//if (!response.Success) {
				Log.ErrorFormat("SalesForce ContactModel failed for customer {0}, error: {1}", model.Email, result);
			//}
		}

		public void CreateTask(TaskModel model) {
			Log.InfoFormat("SalesForce CreateTask\n {0}", model.ToStringExtension());
			string result = "";
			//var response = api.CreateTask(model);//TODO
			//if (!response.Success) {
				Log.ErrorFormat("SalesForce CreateTask failed for customer {0}, error: {1}", model.Email, result);
			//}
		}

		public void CreateActivity(ActivityModel model) {
			Log.InfoFormat("SalesForce CreateEvent\n {0}", model.ToStringExtension());
			string result = "";
			//var response = api.CreateActivity(model);//TODO
			//if (!response.Success) {
				Log.ErrorFormat("SalesForce CreateEvent failed for customer {0}, error: {1}", model.Email, result);
			//}
		}

		public void ChangeEmail(string currentEmail, string newEmail) {
			Log.InfoFormat("SalesForce ChangeEmail from {0} to {1}", currentEmail, newEmail);
			string result = "";
			//var response = api.(currentEmail, newEmail);//TODO
			//if (!response.Success) {
				Log.ErrorFormat("SalesForce ChangeEmail failed from {0} to {1} failed, error: {2}", currentEmail, newEmail, result);
			//}
		}


		private readonly EzbobWebServicesPortTypeClient api;
		private readonly ILog Log = LogManager.GetLogger(typeof (SalesForceApiClient));
	}
}
