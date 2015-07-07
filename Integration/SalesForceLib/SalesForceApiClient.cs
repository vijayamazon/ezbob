namespace SalesForceLib {
	using System;
	using System.Collections.Generic;
	using log4net;
	using SalesForceLib.Models;
	using SalesForceLib.SalesForcePartnersServiceNS;
	using SalesForceLib.SalesForceServiceNS;

	public class SalesForceApiClient : ISalesForceAppClient {
		public string Error { get; set; }

		public bool HasError {
			get {
				return !string.IsNullOrEmpty(Error);
			}
		}

		public string Model { get; set; }

		public SalesForceApiClient(string userName, string password, string token, string environment) {
			this.api = new EzbobWebServicesPortTypeClient("EzbobWebServices" + environment);
			this.partnersClient = new SoapClient("PartnersServices" + environment);
			Error = string.Empty;
			Login(userName, password, token);
		}

		private void Login(string userName, string password, string token) {
			this.lr = null;
			try {
				var response = this.partnersClient.login(new loginRequest {
					username = userName,
					password = password + token,
					CallOptions = new SalesForceLib.SalesForcePartnersServiceNS.CallOptions(),
					LoginScopeHeader = new LoginScopeHeader(),
				});
				this.lr = response.result;
			} catch (Exception ex) {
				Error = string.Format("Failed to login to sales force partners server \n{0}", ex);
				Log.Error(Error);
				return;
			}

			if (this.lr != null && this.lr.passwordExpired) {
				Error = "Sales Force: Your password is expired.";
				Log.Error(Error);
			}
		}

		public void CreateUpdateLeadAccount(LeadAccountModel model) {
			string modelStr = model.ToJsonExtension();
			Log.InfoFormat("SalesForce CreateUpdateLeadAccount\n {0}", modelStr);
			string result = null;

			if (this.lr == null || string.IsNullOrEmpty(this.lr.sessionId)) {
				Log.ErrorFormat("SalesForce CreateUpdateLeadAccount null session id");
			} else {
				var response = this.api.LeadAccountService(
					new SalesForceServiceNS.SessionHeader {
						sessionId = this.lr.sessionId
					},
					new SalesForceServiceNS.CallOptions(),
					new SalesForceServiceNS.DebuggingHeader(),
					new SalesForceServiceNS.AllowFieldTruncationHeader(),
					modelStr,
					out result);
			}

			LogResult("LeadAccountService", result, modelStr, model.Email);
		}

		public void CreateOpportunity(OpportunityModel model) {
			string modelStr = model.ToJsonExtension();
			Log.InfoFormat("SalesForce CreateOpportunity\n {0}", modelStr);
			string result = null;
			if (this.lr == null || string.IsNullOrEmpty(this.lr.sessionId)) {
				Log.ErrorFormat("SalesForce CreateOpportunity null session id");
			} else {


				var response = this.api.CreateOpportunityService(
					new SalesForceServiceNS.SessionHeader {
						sessionId = this.lr.sessionId
					},
					new SalesForceServiceNS.CallOptions(),
					new SalesForceServiceNS.DebuggingHeader(),
					new SalesForceServiceNS.AllowFieldTruncationHeader(),
					modelStr,
					out result);
			}
			LogResult("CreateOpportunityService", result, modelStr, model.Email);

		}

		public void UpdateOpportunity(OpportunityModel model) {
			string modelStr = model.ToJsonExtension();
			Log.InfoFormat("SalesForce UpdateCloseOpportunityService\n {0}", modelStr);
			string result = null;
			if (this.lr == null || string.IsNullOrEmpty(this.lr.sessionId)) {
				Log.ErrorFormat("SalesForce UpdateCloseOpportunityService null session id");
			} else {
				// max length of deal lost reason is 255
				const int maxDealLostReasonLength = 255;
				if (model.DealLostReason != null && model.DealLostReason.Length > maxDealLostReasonLength) {
					model.DealLostReason = model.DealLostReason.Substring(0, maxDealLostReasonLength);
				}
				var response = this.api.UpdateCloseOpportunityService(
					new SalesForceServiceNS.SessionHeader {
						sessionId = this.lr.sessionId
					},
					new SalesForceServiceNS.CallOptions(),
					new SalesForceServiceNS.DebuggingHeader(),
					new SalesForceServiceNS.AllowFieldTruncationHeader(),
					modelStr,
					out result);
			}
			LogResult("UpdateCloseOpportunityService", result, modelStr, model.Email);

		}

		public void CreateUpdateContact(ContactModel model) {
			string modelStr = model.ToJsonExtension();
			Log.InfoFormat("SalesForce CreateUpdateContact\n {0}", modelStr);
			string result = null;
			if (this.lr == null || string.IsNullOrEmpty(this.lr.sessionId)) {
				Log.ErrorFormat("SalesForce CreateUpdateContact null session id");
				return;
			} else {
				var response = this.api.ContactService(
					new SalesForceServiceNS.SessionHeader {
						sessionId = this.lr.sessionId
					},
					new SalesForceServiceNS.CallOptions(),
					new SalesForceServiceNS.DebuggingHeader(),
					new SalesForceServiceNS.AllowFieldTruncationHeader(),
					modelStr,
					out result);
			}
			LogResult("ContactService", result, modelStr, model.Email);

		}

		public void CreateTask(TaskModel model) {
			string modelStr = model.ToJsonExtension();
			Log.InfoFormat("SalesForce CreateTask\n {0}", modelStr);
			string result = null;
			if (this.lr == null || string.IsNullOrEmpty(this.lr.sessionId)) {
				Log.ErrorFormat("SalesForce CreateTask null session id");
			} else {
				var response = this.api.CreateTask(
					new SalesForceServiceNS.SessionHeader {
						sessionId = this.lr.sessionId
					},
					new SalesForceServiceNS.CallOptions(),
					new SalesForceServiceNS.DebuggingHeader(),
					new SalesForceServiceNS.AllowFieldTruncationHeader(),
					modelStr,
					out result);
			}
			LogResult("CreateTask", result, modelStr, model.Email);

		}

		public void CreateActivity(ActivityModel model) {
			string modelStr = model.ToJsonExtension();
			Log.InfoFormat("SalesForce CreateActivity\n {0}", modelStr);
			string result = null;
			if (this.lr == null || string.IsNullOrEmpty(this.lr.sessionId)) {
				Log.ErrorFormat("SalesForce CreateActivity null session id");
			} else {
				var response = this.api.CreateActivity(
					new SalesForceServiceNS.SessionHeader {
						sessionId = this.lr.sessionId
					},
					new SalesForceServiceNS.CallOptions(),
					new SalesForceServiceNS.DebuggingHeader(),
					new SalesForceServiceNS.AllowFieldTruncationHeader(),
					modelStr,
					out result);
			}
			LogResult("CreateActivity", result, modelStr, model.Email);
		}

		public void ChangeEmail(string currentEmail, string newEmail) {
			Log.InfoFormat("SalesForce ChangeEmail from {0} to {1}", currentEmail, newEmail);
			string result = null;
			if (this.lr == null || string.IsNullOrEmpty(this.lr.sessionId)) {
				Log.ErrorFormat("SalesForce ChangeEmail null session id");
			} else {
				var response = this.api.ChangeEmail(
					new SalesForceServiceNS.SessionHeader {
						sessionId = this.lr.sessionId
					},
					new SalesForceServiceNS.CallOptions(),
					new SalesForceServiceNS.DebuggingHeader(),
					new SalesForceServiceNS.AllowFieldTruncationHeader(),
					new {
						currentEmail,
						newEmail
					}.ToJsonExtension(),
					out result);
			}
			LogResult("ChangeEmail", result, new { currentEmail, newEmail }.ToJsonExtension(), newEmail);
		}

		public GetActivityResultModel GetActivity(string email) {
			Log.InfoFormat("SalesForce GetActivity for {0}", email);
			string result = null;
			if (this.lr == null || string.IsNullOrEmpty(this.lr.sessionId)) {
				Log.ErrorFormat("SalesForce GetActivity null session id");
			} else {
				var response = this.api.GetActivity(
					new SalesForceServiceNS.SessionHeader {
						sessionId = this.lr.sessionId
					},
					new SalesForceServiceNS.CallOptions(),
					new SalesForceServiceNS.DebuggingHeader(),
					new SalesForceServiceNS.AllowFieldTruncationHeader(),
					new {
						Email = email
					}.ToJsonExtension(),
					out result);
			}
			LogResult("GetActivity", result, new { email }.ToJsonExtension(), email);
			var res = result.JsonStringToObject<ApiResponse>(true);
			try {
				var activities = res.Success.Replace("\\", "").JsonStringToObject<IEnumerable<ActivityResultModel>>(true);
				return new GetActivityResultModel(activities, res.Error);
			} catch (Exception) {
				Error = string.Format("Failed parsing activity model {0}", res.Success);
				Log.ErrorFormat(Error);
			}

			return new GetActivityResultModel { Error = Error };
		}

		private void LogResult(string serviceName, string result, string request, string email) {
			var res = string.IsNullOrEmpty(result) ? new ApiResponse(null, "result is null") : result.JsonStringToObject<ApiResponse>();
			if (!res.IsSuccess) {
				Log.ErrorFormat("SalesForce {3} failed for customer {0}, request \n{2}\n error: {1}", email, res.Error, request, serviceName);
				Model = request;
				Error = res.Error;
			} else {
				Error = String.Empty;
				Log.InfoFormat("SalesForce {3} success for customer {0}, request \n{2}\n response: {1}", email, result, request, serviceName);
			}
		}

		private readonly EzbobWebServicesPortTypeClient api;
		private readonly Soap partnersClient;
		protected static readonly ILog Log = LogManager.GetLogger(typeof(SalesForceApiClient));
		private LoginResult lr;
	}
}
