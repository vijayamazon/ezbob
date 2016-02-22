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
			this.loginResult = null;
			try {
				var response = this.partnersClient.login(new loginRequest {
					username = this.userName,
					password = this.password + this.token,
					CallOptions = new SalesForceLib.SalesForcePartnersServiceNS.CallOptions(),
					LoginScopeHeader = new LoginScopeHeader(),
				});
				this.loginResult = response.result;
			} catch (Exception ex) {
				Error = string.Format("Failed to login to sales force partners server \n{0}", ex);
				Log.Error(Error);
				HasLoginError = true;
				return;
			}

			if (this.loginResult != null && this.loginResult.passwordExpired) {
				Error = "Sales Force: Your password is expired.";
				Log.Error(Error);
				HasLoginError = true;
			}

			if (this.loginResult == null || string.IsNullOrEmpty(this.loginResult.sessionId)) {
				Log.ErrorFormat("SalesForce Login null session id");
				HasLoginError = true;
			}
		}

		public void CreateUpdateLeadAccount(LeadAccountModel model) {
			string result = null;
			try {
				Model = model.ToJsonExtension();
				if (this.loginResult != null && !string.IsNullOrEmpty(this.loginResult.sessionId)) {
					var response = this.api.LeadAccountService(
						new SalesForceServiceNS.SessionHeader {
							sessionId = this.loginResult.sessionId
						},
						new SalesForceServiceNS.CallOptions(),
						new SalesForceServiceNS.DebuggingHeader(),
						new SalesForceServiceNS.AllowFieldTruncationHeader(),
						Model,
						out result);

					Log.DebugFormat("Debug log: {0}", response == null ? "" : response.debugLog);
				}
			} catch (Exception ex) {
				var resultModel = new ApiResponse(null, ex.Message);
				result = resultModel.ToJsonExtension();
			}
			LogResult("LeadAccountService", result, model.Email);
		}

		public void CreateOpportunity(OpportunityModel model) {
			string result = null;
			try {
				Model = model.ToJsonExtension();
				if (this.loginResult != null && !string.IsNullOrEmpty(this.loginResult.sessionId)) {
					var response = this.api.CreateOpportunityService(
						new SalesForceServiceNS.SessionHeader {
							sessionId = this.loginResult.sessionId,
						},
						new SalesForceServiceNS.CallOptions(),
						new SalesForceServiceNS.DebuggingHeader(),
						new SalesForceServiceNS.AllowFieldTruncationHeader(),
						Model,
						out result);

					Log.DebugFormat("Debug log: {0}", response == null ? "" : response.debugLog);
				}
			} catch (Exception ex) {
				var resultModel = new ApiResponse(null, ex.Message);
				result = resultModel.ToJsonExtension();
			}
			LogResult("CreateOpportunityService", result, model.Email);

		}

		public void UpdateOpportunity(OpportunityModel model) {
			string result = null;
			try {
				Model = model.ToJsonExtension();
				if (this.loginResult != null && !string.IsNullOrEmpty(this.loginResult.sessionId)) {
					// max length of deal lost reason is 255
					const int maxDealLostReasonLength = 255;
					if (model.DealLostReason != null && model.DealLostReason.Length > maxDealLostReasonLength) {
						model.DealLostReason = model.DealLostReason.Substring(0, maxDealLostReasonLength);
					}
					var response = this.api.UpdateCloseOpportunityService(
						new SalesForceServiceNS.SessionHeader {
							sessionId = this.loginResult.sessionId
						},
						new SalesForceServiceNS.CallOptions(),
						new SalesForceServiceNS.DebuggingHeader(),
						new SalesForceServiceNS.AllowFieldTruncationHeader(),
						Model,
						out result);

					Log.DebugFormat("Debug log: {0}", response == null ? "" : response.debugLog);
				}
			} catch (Exception ex) {
				var resultModel = new ApiResponse(null, ex.Message);
				result = resultModel.ToJsonExtension();
			}
			LogResult("UpdateCloseOpportunityService", result, model.Email);
		}

		public void CreateUpdateContact(ContactModel model) {
			string result = null;
			try {
				Model = model.ToJsonExtension();
				if (this.loginResult != null && !string.IsNullOrEmpty(this.loginResult.sessionId)) {
					var response = this.api.ContactService(
						new SalesForceServiceNS.SessionHeader {
							sessionId = this.loginResult.sessionId
						},
						new SalesForceServiceNS.CallOptions(),
						new SalesForceServiceNS.DebuggingHeader(),
						new SalesForceServiceNS.AllowFieldTruncationHeader(),
						Model,
						out result);

					Log.DebugFormat("Debug log: {0}", response == null ? "" : response.debugLog);
				}
			} catch (Exception ex) {
				var resultModel = new ApiResponse(null, ex.Message);
				result = resultModel.ToJsonExtension();
			}
			LogResult("ContactService", result, model.Email);
		}

		public void CreateTask(TaskModel model) {
			string result = null;
			try {
				Model = model.ToJsonExtension();
				if (this.loginResult != null && !string.IsNullOrEmpty(this.loginResult.sessionId)) {
					var response = this.api.CreateTask(
						new SalesForceServiceNS.SessionHeader {
							sessionId = this.loginResult.sessionId
						},
						new SalesForceServiceNS.CallOptions(),
						new SalesForceServiceNS.DebuggingHeader(),
						new SalesForceServiceNS.AllowFieldTruncationHeader(),
						Model,
						out result);

					Log.DebugFormat("Debug log: {0}", response == null ? "" : response.debugLog);
				}
			} catch (Exception ex) {
				var resultModel = new ApiResponse(null, ex.Message);
				result = resultModel.ToJsonExtension();
			}
			LogResult("CreateTask", result, model.Email);

		}

		public void CreateActivity(ActivityModel model) {
			string result = null;
			try {
				Model = model.ToJsonExtension();
				if (this.loginResult != null && !string.IsNullOrEmpty(this.loginResult.sessionId)) {
					var response = this.api.CreateActivity(
						new SalesForceServiceNS.SessionHeader {
							sessionId = this.loginResult.sessionId
						},
						new SalesForceServiceNS.CallOptions(),
						new SalesForceServiceNS.DebuggingHeader(),
						new SalesForceServiceNS.AllowFieldTruncationHeader(),
						Model,
						out result);

					Log.DebugFormat("Debug log: {0}", response == null ? "" : response.debugLog);
				}
			} catch (Exception ex) {
				var resultModel = new ApiResponse(null, ex.Message);
				result = resultModel.ToJsonExtension();
			}
			LogResult("CreateActivity", result, model.Email);
		}

		public void ChangeEmail(ChangeEmailModel model) {
			string result = null;
			try {
				Model = model.ToJsonExtension();
				if (this.loginResult != null && !string.IsNullOrEmpty(this.loginResult.sessionId)) {
					var response = this.api.ChangeEmail(
						new SalesForceServiceNS.SessionHeader {
							sessionId = this.loginResult.sessionId
						},
						new SalesForceServiceNS.CallOptions(),
						new SalesForceServiceNS.DebuggingHeader(),
						new SalesForceServiceNS.AllowFieldTruncationHeader(),
						Model,
						out result);

					Log.DebugFormat("Debug log: {0}", response == null ? "" : response.debugLog);
				}
			} catch (Exception ex) {
				var resultModel = new ApiResponse(null, ex.Message);
				result = resultModel.ToJsonExtension();
			}
			LogResult("ChangeEmail", result, model.newEmail);
		}

		public GetActivityResultModel GetActivity(GetActivityModel model) {
			string result = null;
			try {
				Model = model.ToJsonExtension();
				if (this.loginResult != null && !string.IsNullOrEmpty(this.loginResult.sessionId)) {
					var response = this.api.GetActivity(
						new SalesForceServiceNS.SessionHeader {
							sessionId = this.loginResult.sessionId
						},
						new SalesForceServiceNS.CallOptions(),
						new SalesForceServiceNS.DebuggingHeader(),
						new SalesForceServiceNS.AllowFieldTruncationHeader(),
						Model,
						out result);

					Log.DebugFormat("Debug log: {0}", response == null ? "" : response.debugLog);
				}
			} catch (Exception ex) {
				var resultModel = new ApiResponse(null, ex.Message);
				result = resultModel.ToJsonExtension();
			}
			LogResult("GetActivity", result, model.Email);
			try {
				var res = result.JsonStringToObject<ApiResponse>(true);
				if (res.Success == null) { res.Success = String.Empty; }
				var activities = res.Success.Replace("\\", "").JsonStringToObject<IEnumerable<ActivityResultModel>>(true);
				return new GetActivityResultModel(activities, res.Error);
			} catch (Exception) {
				Error = "Failed parsing activity model\n" + result;
				Log.Warn(Error);
			}

			return new GetActivityResultModel { Error = Error };
		}

		private void LogResult(string serviceName, string result, string email) {
			try {
				var res = string.IsNullOrEmpty(result) ? new ApiResponse(null, "result is null") : result.JsonStringToObject<ApiResponse>();
				if (!res.IsSuccess) {
					string message = "SalesForce " + serviceName + " failed for customer " + email + ", request \n" + Model + "\n error: " + res.Error + "";
					Log.Warn(message);
					Error = res.Error;
				} else {
					Error = String.Empty;
					string message = "SalesForce " + serviceName + " success for customer " + email + ", request \n" + Model + "\n response:" + res.Success;
					Log.Info(message);
				}
			} catch (Exception ex) {
				Error = "Failed parsing result to object " + result;
				string message = "SalesForce " + serviceName + " failed for customer " + email + ", request \n" + Model + "\n error: failed parsing response:\n" + result + "";
				Log.Warn(message);
				Log.Warn(ex);
			}
		}

		private readonly EzbobWebServicesPortTypeClient api;
		private readonly Soap partnersClient;
		protected static readonly ILog Log = LogManager.GetLogger(typeof(SalesForceApiClient));
		private LoginResult loginResult;
		private readonly string token;
		private readonly string password;
		private readonly string userName;
	}
}
