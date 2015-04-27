namespace SalesForceLib {
    using System;
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
                this.Log.Error(Error);
                return;
            }

            if (this.lr != null && this.lr.passwordExpired) {
                Error = "Sales Force: Your password is expired.";
                this.Log.Error(Error);
            }
        }

        public void CreateUpdateLeadAccount(LeadAccountModel model) {
            string modelStr = model.ToJsonExtension();
            this.Log.InfoFormat("SalesForce CreateUpdateLeadAccount\n {0}", modelStr);

            if (this.lr == null || string.IsNullOrEmpty(this.lr.sessionId)) {
                this.Log.ErrorFormat("SalesForce CreateUpdateLeadAccount null session id");
                return;
            }

            string result;
            var response = this.api.LeadAccountService(
                new SalesForceServiceNS.SessionHeader {
                    sessionId = this.lr.sessionId
                },
                new SalesForceServiceNS.CallOptions(),
                new SalesForceServiceNS.DebuggingHeader(),
                new SalesForceServiceNS.AllowFieldTruncationHeader(),
                modelStr,
                out result);


            LogResult("LeadAccountService", result, modelStr, model.Email);


        }

        public void CreateOpportunity(OpportunityModel model) {
            string modelStr = model.ToJsonExtension();
            this.Log.InfoFormat("SalesForce CreateOpportunity\n {0}", modelStr);

            if (this.lr == null || string.IsNullOrEmpty(this.lr.sessionId)) {
                this.Log.ErrorFormat("SalesForce CreateOpportunity null session id");
                return;
            }

            string result;
            var response = this.api.CreateOpportunityService(
                new SalesForceServiceNS.SessionHeader {
                    sessionId = this.lr.sessionId
                },
                new SalesForceServiceNS.CallOptions(),
                new SalesForceServiceNS.DebuggingHeader(),
                new SalesForceServiceNS.AllowFieldTruncationHeader(),
                modelStr,
                out result);

            LogResult("CreateOpportunityService", result, modelStr, model.Email);

        }

        public void UpdateOpportunity(OpportunityModel model) {
            string modelStr = model.ToJsonExtension();
            this.Log.InfoFormat("SalesForce UpdateCloseOpportunityService\n {0}", modelStr);

            if (this.lr == null || string.IsNullOrEmpty(this.lr.sessionId)) {
                this.Log.ErrorFormat("SalesForce UpdateCloseOpportunityService null session id");
                return;
            }
            // max length of deal lost reason is 255
            const int maxDealLostReasonLength = 255;
            if (model.DealLostReason != null && model.DealLostReason.Length > maxDealLostReasonLength) {
                model.DealLostReason = model.DealLostReason.Substring(0, maxDealLostReasonLength);
            }

            string result;
            var response = this.api.UpdateCloseOpportunityService(
                new SalesForceServiceNS.SessionHeader {
                    sessionId = this.lr.sessionId
                },
                new SalesForceServiceNS.CallOptions(),
                new SalesForceServiceNS.DebuggingHeader(),
                new SalesForceServiceNS.AllowFieldTruncationHeader(),
                modelStr,
                out result);

            LogResult("UpdateCloseOpportunityService", result, modelStr, model.Email);

        }

        public void CreateUpdateContact(ContactModel model) {
            string modelStr = model.ToJsonExtension();
            this.Log.InfoFormat("SalesForce CreateUpdateContact\n {0}", modelStr);

            if (this.lr == null || string.IsNullOrEmpty(this.lr.sessionId)) {
                this.Log.ErrorFormat("SalesForce CreateUpdateContact null session id");
                return;
            }

            string result;
            var response = this.api.ContactService(
                new SalesForceServiceNS.SessionHeader {
                    sessionId = this.lr.sessionId
                },
                new SalesForceServiceNS.CallOptions(),
                new SalesForceServiceNS.DebuggingHeader(),
                new SalesForceServiceNS.AllowFieldTruncationHeader(),
                modelStr,
                out result);

            LogResult("ContactService", result, modelStr, model.Email);

        }

        public void CreateTask(TaskModel model) {
            string modelStr = model.ToJsonExtension();
            this.Log.InfoFormat("SalesForce CreateTask\n {0}", modelStr);

            if (this.lr == null || string.IsNullOrEmpty(this.lr.sessionId)) {
                this.Log.ErrorFormat("SalesForce CreateTask null session id");
                return;
            }

            string result = "";
            var response = this.api.CreateTask(
                new SalesForceServiceNS.SessionHeader {
                    sessionId = this.lr.sessionId
                },
                new SalesForceServiceNS.CallOptions(),
                new SalesForceServiceNS.DebuggingHeader(),
                new SalesForceServiceNS.AllowFieldTruncationHeader(),
                modelStr,
                out result);

            LogResult("CreateTask", result, modelStr, model.Email);

        }

        public void CreateActivity(ActivityModel model) {
            string modelStr = model.ToJsonExtension(true);
            this.Log.InfoFormat("SalesForce CreateActivity\n {0}", modelStr);

            if (this.lr == null || string.IsNullOrEmpty(this.lr.sessionId)) {
                this.Log.ErrorFormat("SalesForce CreateActivity null session id");
                return;
            }

            string result = "";
            this.api.CreateActivity(
                new SalesForceServiceNS.SessionHeader {
                    sessionId = this.lr.sessionId
                },
                new SalesForceServiceNS.CallOptions(),
                new SalesForceServiceNS.DebuggingHeader(),
                new SalesForceServiceNS.AllowFieldTruncationHeader(),
                modelStr,
                out result);

            LogResult("CreateActivity", result, modelStr, model.Email);
        }

        public void ChangeEmail(string currentEmail, string newEmail) {
            this.Log.InfoFormat("SalesForce ChangeEmail from {0} to {1}", currentEmail, newEmail);

            if (this.lr == null || string.IsNullOrEmpty(this.lr.sessionId)) {
                this.Log.ErrorFormat("SalesForce ChangeEmail null session id");
                return;
            }

            string result = "";
            var response = this.api.ChangeEmail(
                new SalesForceServiceNS.SessionHeader {
                    sessionId = this.lr.sessionId
                },
                new SalesForceServiceNS.CallOptions(),
                new SalesForceServiceNS.DebuggingHeader(),
                new SalesForceServiceNS.AllowFieldTruncationHeader(),
                new { currentEmail, newEmail }.ToJsonExtension(),
                out result);

            LogResult("ChangeEmail", result, new { currentEmail, newEmail }.ToJsonExtension(), newEmail);
        }

        private void LogResult(string serviceName, string result, string request, string email) {
            var res = result.JsonStringToObject<ApiResponse>();
            if (!res.IsSuccess) {
                this.Log.ErrorFormat("SalesForce {3} failed for customer {0}, request \n{2}\n error: {1}", email, res.Error, request, serviceName);
                Model = request;
                Error = res.Error;
            } else {
                this.Log.InfoFormat("SalesForce {3} success for customer {0}, request \n{2}\n response: {1}", email, result, request, serviceName);
            }
        }

        private readonly EzbobWebServicesPortTypeClient api;
        private readonly Soap partnersClient;
        private readonly ILog Log = LogManager.GetLogger(typeof(SalesForceApiClient));
        private LoginResult lr;
    }
}
