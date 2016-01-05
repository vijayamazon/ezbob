namespace SalesForceLib
{
	using System;
	using System.Collections.Generic;
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

		public bool HasLoginError { get; private set; }

		public string Model { get; set; }

		public FakeApiClient(string userName = "", string password = "", string token = "", string environment = "") {
			this.api = new Api();
		}

		public void CreateUpdateLeadAccount(LeadAccountModel model) {
			HasLoginError = false;
			Log.InfoFormat("SalesForce CreateUpdateLeadAccount\n {0}", model.ToStringExtension());
			ApiResponse response = this.api.CreateUpdateLeadAccount(model);
			if (!response.IsSuccess) {
				Log.ErrorFormat("SalesForce CreateUpdateLeadAccount failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void Login() {

		}

		public void CreateOpportunity(OpportunityModel model) {
			Log.InfoFormat("SalesForce CreateOpportunity\n {0}", model.ToStringExtension());
			ApiResponse response = this.api.CreateOpportunity(model);
			if (!response.IsSuccess) {
				Log.ErrorFormat("SalesForce CreateOpportunity failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void UpdateOpportunity(OpportunityModel model) {
			Log.InfoFormat("SalesForce UpdateOpportunity\n {0}", model.ToStringExtension());
			ApiResponse response = this.api.UpdateOpportunity(model);
			if (!response.IsSuccess) {
				Log.ErrorFormat("SalesForce UpdateOpportunity failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void CreateUpdateContact(ContactModel model) {
			Log.InfoFormat("SalesForce CreateUpdateContact\n {0}", model.ToStringExtension());
			ApiResponse response = this.api.CreateUpdateContact(model);
			if (!response.IsSuccess) {
				Log.ErrorFormat("SalesForce ContactModel failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void CreateTask(TaskModel model) {
			Log.InfoFormat("SalesForce CreateTask\n {0}", model.ToStringExtension());
			ApiResponse response = this.api.CreateTask(model);
			if (!response.IsSuccess) {
				Log.ErrorFormat("SalesForce CreateTask failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void CreateActivity(ActivityModel model) {
			Log.InfoFormat("SalesForce CreateActivity\n {0}", model.ToStringExtension());
			ApiResponse response = this.api.CreateActivity(model);
			if (!response.IsSuccess) {
				Log.ErrorFormat("SalesForce CreateActivity failed for customer {0}, error: {1}", model.Email, response.Error);
			}
		}

		public void ChangeEmail(ChangeEmailModel model) {
			Log.InfoFormat("SalesForce ChangeEmail from {0} to {1}", model.currentEmail, model.newEmail);
			ApiResponse response = this.api.ChangeEmail(model);
			if (!response.IsSuccess) {
				Log.ErrorFormat("SalesForce ChangeEmail failed from {0} to {1} failed, error: {2}", model.currentEmail, model.newEmail, response.Error);
			}
		}

		public GetActivityResultModel GetActivity(GetActivityModel model) {
			Log.InfoFormat("SalesForce GetActivity for {0}", model.Email);
			ApiResponse response = this.api.GetActivity(model);

			if (!string.IsNullOrEmpty(response.Success)) {
				try {
					var activities = response.Success.Replace("\\", "").JsonStringToObject<IEnumerable<ActivityResultModel>>();
					return new GetActivityResultModel(activities, response.Error);
				} catch (Exception) {
					Error = string.Format("Failed parsing activity model {0}", response.Success);
					Log.ErrorFormat(Error);
				}
			}
			return null;
		}

		private readonly Api api;
		protected static readonly ILog Log = LogManager.GetLogger(typeof (FakeApiClient));
	}
}
