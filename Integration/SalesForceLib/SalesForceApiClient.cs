namespace SalesForceLib {
	using System;
	using System.Collections.Generic;
	using log4net;
	using SalesForceLib.Models;
	using SalesForceLib.SalesForcePartnersServiceNS;
	using SalesForceLib.SalesForceServiceNS;

	public class SalesForceApiClient : ISalesForceAppClient {
		public string Error { get; private set; }
		public bool HasLoginError { get; private set; }
		public bool HasError { get { return !string.IsNullOrEmpty(Error) || HasLoginError; } }
		public string Model { get; set; }

		public SalesForceApiClient(string userName, string password, string token, string environment) {
			this.api = new EzbobWebServicesPortTypeClient("EzbobWebServices" + environment);
			this.partnersClient = new SoapClient("PartnersServices" + environment);
			Error = string.Empty;
			this.userName = userName;
			this.password = password;
			this.token = token;
			Login();
		}

		public void Login() {
			HasLoginError = false;
			this.lr = null;
			try {
				var response = this.partnersClient.login(new loginRequest {
					username = this.userName,
					password = this.password + this.token,
					CallOptions = new SalesForceLib.SalesForcePartnersServiceNS.CallOptions(),
					LoginScopeHeader = new LoginScopeHeader(),
				});
				this.lr = response.result;
			} catch (Exception ex) {
				Error = string.Format("Failed to login to sales force partners server \n{0}", ex);
				Log.Error(Error);
				HasLoginError = true;
				return;
			}

			if (this.lr != null && this.lr.passwordExpired) {
				Error = "Sales Force: Your password is expired.";
				Log.Error(Error);
				HasLoginError = true;
			}

			if (this.lr == null || string.IsNullOrEmpty(this.lr.sessionId)) {
				Log.ErrorFormat("SalesForce Login null session id");
				HasLoginError = true;
			}
		}

		public void CreateUpdateLeadAccount(LeadAccountModel model) {
			Model = model.ToJsonExtension();
			string result = null;

			if (this.lr != null && !string.IsNullOrEmpty(this.lr.sessionId)) {
				var response = this.api.LeadAccountService(
					new SalesForceServiceNS.SessionHeader {
						sessionId = this.lr.sessionId
					},
					new SalesForceServiceNS.CallOptions(),
					new SalesForceServiceNS.DebuggingHeader(),
					new SalesForceServiceNS.AllowFieldTruncationHeader(),
					Model,
					out result);

				Log.DebugFormat("Debug log: {0}", response == null ? "" : response.debugLog);
			}

			LogResult("LeadAccountService", result, model.Email);
		}

		public void CreateOpportunity(OpportunityModel model) {
			Model = model.ToJsonExtension();
			string result = null;
			if (this.lr != null && !string.IsNullOrEmpty(this.lr.sessionId)) {
				var response = this.api.CreateOpportunityService(
					new SalesForceServiceNS.SessionHeader {
						sessionId = this.lr.sessionId
					},
					new SalesForceServiceNS.CallOptions(),
					new SalesForceServiceNS.DebuggingHeader(),
					new SalesForceServiceNS.AllowFieldTruncationHeader(),
					Model,
					out result);

				Log.DebugFormat("Debug log: {0}", response == null ? "" : response.debugLog);
			}
			LogResult("CreateOpportunityService", result, model.Email);

		}

		public void UpdateOpportunity(OpportunityModel model) {
			Model = model.ToJsonExtension();
			string result = null;
			if (this.lr != null && !string.IsNullOrEmpty(this.lr.sessionId)) {
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
					Model,
					out result);

				Log.DebugFormat("Debug log: {0}", response == null ? "" : response.debugLog);
			}
			LogResult("UpdateCloseOpportunityService", result, model.Email);
		}

		public void CreateUpdateContact(ContactModel model) {
			Model = model.ToJsonExtension();
			string result = null;
			if (this.lr != null && !string.IsNullOrEmpty(this.lr.sessionId)) {
				var response = this.api.ContactService(
					new SalesForceServiceNS.SessionHeader {
						sessionId = this.lr.sessionId
					},
					new SalesForceServiceNS.CallOptions(),
					new SalesForceServiceNS.DebuggingHeader(),
					new SalesForceServiceNS.AllowFieldTruncationHeader(),
					Model,
					out result);

				Log.DebugFormat("Debug log: {0}", response == null ? "" : response.debugLog);
			}
			LogResult("ContactService", result, model.Email);
		}

		public void CreateTask(TaskModel model) {
			Model = model.ToJsonExtension();
			string result = null;
			if (this.lr != null && !string.IsNullOrEmpty(this.lr.sessionId)) {
				var response = this.api.CreateTask(
					new SalesForceServiceNS.SessionHeader {
						sessionId = this.lr.sessionId
					},
					new SalesForceServiceNS.CallOptions(),
					new SalesForceServiceNS.DebuggingHeader(),
					new SalesForceServiceNS.AllowFieldTruncationHeader(),
					Model,
					out result);

				Log.DebugFormat("Debug log: {0}", response == null ? "" : response.debugLog);
			}
			LogResult("CreateTask", result, model.Email);

		}

		public void CreateActivity(ActivityModel model) {
			Model = model.ToJsonExtension();
			string result = null;
			if (this.lr != null && !string.IsNullOrEmpty(this.lr.sessionId)) {
				var response = this.api.CreateActivity(
					new SalesForceServiceNS.SessionHeader {
						sessionId = this.lr.sessionId
					},
					new SalesForceServiceNS.CallOptions(),
					new SalesForceServiceNS.DebuggingHeader(),
					new SalesForceServiceNS.AllowFieldTruncationHeader(),
					Model,
					out result);

				Log.DebugFormat("Debug log: {0}", response == null ? "" : response.debugLog);
			}
			LogResult("CreateActivity", result, model.Email);
		}

		public void ChangeEmail(string currentEmail, string newEmail) {
			Model=new { currentEmail, newEmail }.ToJsonExtension();
			string result = null;
			if (this.lr != null && !string.IsNullOrEmpty(this.lr.sessionId)) {
				var response = this.api.ChangeEmail(
					new SalesForceServiceNS.SessionHeader {
						sessionId = this.lr.sessionId
					},
					new SalesForceServiceNS.CallOptions(),
					new SalesForceServiceNS.DebuggingHeader(),
					new SalesForceServiceNS.AllowFieldTruncationHeader(),
					Model,
					out result);

				Log.DebugFormat("Debug log: {0}", response == null ? "" : response.debugLog);
			}
			LogResult("ChangeEmail", result, newEmail);
		}

		public GetActivityResultModel GetActivity(string email) {
			Model = new { Email = email }.ToJsonExtension(); 
			string result = null;
			if (this.lr != null && !string.IsNullOrEmpty(this.lr.sessionId)) {
				var response = this.api.GetActivity(
					new SalesForceServiceNS.SessionHeader {
						sessionId = this.lr.sessionId
					},
					new SalesForceServiceNS.CallOptions(),
					new SalesForceServiceNS.DebuggingHeader(),
					new SalesForceServiceNS.AllowFieldTruncationHeader(),
					Model,
					out result);

				Log.DebugFormat("Debug log: {0}", response == null ? "" : response.debugLog);
			}
			LogResult("GetActivity", result, email);
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

		private void LogResult(string serviceName, string result, string email) {
			var res = string.IsNullOrEmpty(result) ? new ApiResponse(null, "result is null") : result.JsonStringToObject<ApiResponse>();
			if (!res.IsSuccess) {
				Log.ErrorFormat("SalesForce {3} failed for customer {0}, request \n{2}\n error: {1}", email, res.Error, Model, serviceName);
				Error = res.Error;
			} else {
				Error = String.Empty;
				Log.InfoFormat("SalesForce {3} success for customer {0}, request \n{2}\n response: {1}", email, result, Model, serviceName);
			}
		}

		private readonly EzbobWebServicesPortTypeClient api;
		private readonly Soap partnersClient;
		protected static readonly ILog Log = LogManager.GetLogger(typeof(SalesForceApiClient));
		private LoginResult lr;
		private readonly string token;
		private readonly string password;
		private readonly string userName;
	}
}
