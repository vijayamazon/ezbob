namespace SalesForceLib {
	using System;
	using log4net;
	using SalesForceLib.Models;
	using SalesForceLib.SalesForcePartnersServiceNS;
	using SalesForceLib.SalesForceServiceNS;

	public class SalesForceApiClient : ISalesForceAppClient {
		public SalesForceApiClient(string userName, string password, string token, string environment) {
			api = new EzbobWebServicesPortTypeClient("EzbobWebServices" + environment);
			partnersClient = new SoapClient("PartnersServices" + environment); 
			Login(userName, password, token);
		}

		private void Login(string userName, string password, string token) {
			lr = null;
			try {
				var response = partnersClient.login(new loginRequest {
					username = userName,
					password = password + token,
					CallOptions = new SalesForceLib.SalesForcePartnersServiceNS.CallOptions(),
					LoginScopeHeader = new LoginScopeHeader(),
				});
				lr = response.result;
			} catch (Exception ex) {
				Log.ErrorFormat("Failed to login to sales force partners server \n{0}", ex);
				return;
			}

			if (lr != null && lr.passwordExpired) {
				Log.Error("Sales Force: Your password is expired.");
			}
		}

		public void CreateUpdateLeadAccount(LeadAccountModel model) {
			Log.InfoFormat("SalesForce CreateUpdateLeadAccount\n {0}", model.ToStringExtension());
			string result;
			var response = api.LeadAccountService(
				new SalesForceServiceNS.SessionHeader {
					sessionId = lr.sessionId
				},
				new SalesForceServiceNS.CallOptions(),
				new SalesForceServiceNS.DebuggingHeader(),
				new SalesForceServiceNS.AllowFieldTruncationHeader(),
				model.ToJsonExtension(),
				out result);

			var res = result.JsonStringToObject<ApiResponse>();
			if (!res.IsSuccess) {
				Log.ErrorFormat("SalesForce CreateUpdateLeadAccount failed for customer {0}, error: {1}", model.Email, res.Error);
			} else {
				Log.InfoFormat("SalesForce CreateUpdateLeadAccount success for customer {0}, {1}", model.Email, result);
			}
		}

		public void CreateOpportunity(OpportunityModel model) {
			Log.InfoFormat("SalesForce CreateOpportunity\n {0}", model.ToStringExtension());
			string result;
			var response = api.CreateOpportunityService(
				new SalesForceServiceNS.SessionHeader {
					sessionId = lr.sessionId
				},
				new SalesForceServiceNS.CallOptions(),
				new SalesForceServiceNS.DebuggingHeader(),
				new SalesForceServiceNS.AllowFieldTruncationHeader(),
				model.ToJsonExtension(),
				out result);
		
			var res = result.JsonStringToObject<ApiResponse>();
			if (!res.IsSuccess) {
				Log.ErrorFormat("SalesForce CreateOpportunity failed for customer {0}, error: {1}", model.Email, res.Error);
			} else {
				Log.InfoFormat("SalesForce CreateOpportunity success for customer {0}, {1}", model.Email, result);
			}
		}

		public void UpdateOpportunity(OpportunityModel model) {
			Log.InfoFormat("SalesForce UpdateOpportunity\n {0}", model.ToStringExtension());
			string result;
			var response = api.UpdateCloseOpportunityService(
				new SalesForceServiceNS.SessionHeader {
					sessionId = lr.sessionId
				},
				new SalesForceServiceNS.CallOptions(),
				new SalesForceServiceNS.DebuggingHeader(),
				new SalesForceServiceNS.AllowFieldTruncationHeader(),
				model.ToJsonExtension(),
				out result);

			var res = result.JsonStringToObject<ApiResponse>();
			if (!res.IsSuccess) {
				Log.ErrorFormat("SalesForce UpdateOpportunity failed for customer {0}, error: {1}", model.Email, res.Error);
			} else {
				Log.InfoFormat("SalesForce UpdateOpportunity success for customer {0}, {1}", model.Email, result);
			}
		}

		public void CreateUpdateContact(ContactModel model) {
			Log.InfoFormat("SalesForce CreateUpdateContact\n {0}", model.ToStringExtension());
			string result;
			var response = api.ContactService(
				new SalesForceServiceNS.SessionHeader {
					sessionId = lr.sessionId
				},
				new SalesForceServiceNS.CallOptions(),
				new SalesForceServiceNS.DebuggingHeader(),
				new SalesForceServiceNS.AllowFieldTruncationHeader(),
				model.ToJsonExtension(),
				out result);
			
			var res = result.JsonStringToObject<ApiResponse>();
			if (!res.IsSuccess) {
				Log.ErrorFormat("SalesForce CreateUpdateContact failed for customer {0}, error: {1}", model.Email, res.Error);
			} else {
				Log.InfoFormat("SalesForce CreateUpdateContact success for customer {0}, {1}", model.Email, result);
			}
		}

		public void CreateTask(TaskModel model) {
			Log.InfoFormat("SalesForce CreateTask\n {0}", model.ToStringExtension());
			string result = "";
			var response = api.CreateTask(
				new SalesForceServiceNS.SessionHeader {
					sessionId = lr.sessionId
				},
				new SalesForceServiceNS.CallOptions(),
				new SalesForceServiceNS.DebuggingHeader(),
				new SalesForceServiceNS.AllowFieldTruncationHeader(),
				model.ToJsonExtension(),
				out result);
			
			var res = result.JsonStringToObject<ApiResponse>();
			if (!res.IsSuccess) {
				Log.ErrorFormat("SalesForce CreateTask failed for customer {0}, error: {1}", model.Email, res.Error);
			} else {
				Log.InfoFormat("SalesForce CreateTask success for customer {0}, {1}", model.Email, result);
			}
		}

		public void CreateActivity(ActivityModel model) {
			Log.InfoFormat("SalesForce CreateEvent\n {0}", model.ToStringExtension());
			string result = "";
			var response = api.CreateActivity(
				new SalesForceServiceNS.SessionHeader {
					sessionId = lr.sessionId
				},
				new SalesForceServiceNS.CallOptions(),
				new SalesForceServiceNS.DebuggingHeader(),
				new SalesForceServiceNS.AllowFieldTruncationHeader(),
				model.ToJsonExtension(),
				out result);
			
			var res = result.JsonStringToObject<ApiResponse>();
			if (!res.IsSuccess) {
				Log.ErrorFormat("SalesForce CreateActivity failed for customer {0}, error: {1}", model.Email, res.Error);
			} else {
				Log.InfoFormat("SalesForce CreateActivity success for customer {0}, {1}", model.Email, result);
			}
		}

		public void ChangeEmail(string currentEmail, string newEmail) {
			Log.InfoFormat("SalesForce ChangeEmail from {0} to {1}", currentEmail, newEmail);
			string result = "";
			var response = api.ChangeEmail(
				new SalesForceServiceNS.SessionHeader {
					sessionId = lr.sessionId
				},
				new SalesForceServiceNS.CallOptions(),
				new SalesForceServiceNS.DebuggingHeader(),
				new SalesForceServiceNS.AllowFieldTruncationHeader(),
				new { currentEmail, newEmail }.ToJsonExtension(),
				out result);
			
			var res = result.JsonStringToObject<ApiResponse>();
			if (!res.IsSuccess) {
				Log.ErrorFormat("SalesForce ChangeEmail failed for customer {0}, error: {1}", newEmail, res.Error);
			} else {
				Log.InfoFormat("SalesForce ChangeEmail success for customer {0}, {1}", newEmail, result);
			}
		}


		private readonly EzbobWebServicesPortTypeClient api;
		private readonly Soap partnersClient;
		private readonly ILog Log = LogManager.GetLogger(typeof(SalesForceApiClient));
		private LoginResult lr;
	}
}
