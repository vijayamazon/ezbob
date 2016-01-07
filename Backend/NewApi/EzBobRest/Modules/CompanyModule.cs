using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobRest.Modules {
    using System.Threading;
    using EzBobApi.Commands.Company;
    using EzBobCommon;
    using EzBobCommon.Utils;
    using EzBobRest.NSB;
    using EzBobRest.ResponseHelpers;
    using EzBobRest.Validators;
    using log4net;
    using Nancy;
    using Nancy.ModelBinding;
    using Nancy.Security;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Handles company related REST requests
    /// </summary>
    public class CompanyModule : NancyModule {

        [Injected]
        public UpdateCompanySendReceive UpdateCompanySendReceive { get; set; }

        [Injected]
        public RestServerConfig Config { get; set; }

        [Injected]
        public CompanyUpdateValidator Validator { get; set; }

        [Injected]
        public ILog Log { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompanyModule"/> class.
        /// </summary>
        public CompanyModule() {
//            this.RequiresMSOwinAuthentication();
            UpdateCompany();
        }

        private void UpdateCompany() {
            Post["UpdateCompany", "api/v1/company/update/{customerId}/{companyId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                //"{companyId}" - is the default value in case when company id is not supplied by request
                string companyId = o.CompanyId != "{companyId}" ? o.CompanyId : null;

                //Bind
                UpdateCompanyCommand updateCommand;
                try {
                    updateCommand = this.Bind<UpdateCompanyCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("error on update company", ex);
                    return CreateErrorResponse(customerId, companyId, ex);
                }

                //Validate
                InfoAccumulator info = Validate(updateCommand);
                if (info.HasErrors) {
                    return CreateErrorResponse(customerId, companyId, null, info.GetErrors());
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendRecieveTaskTimeoutMilis);
                try {
                    var response = await UpdateCompanySendReceive.SendAsync(Config.ServerAddress, updateCommand, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(customerId, companyId, null, response.Errors);
                    }
                } catch (TaskCanceledException) {
                    Log.Error("timeout on update company");
                    return CreateErrorResponse(customerId, companyId, null, null, HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(customerId, companyId);
            };
        }

        /// <summary>
        /// Validates the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        private InfoAccumulator Validate(UpdateCompanyCommand command) {
            var validationResult = Validator.Validate(command);
            if (validationResult.IsValid) {
                return new InfoAccumulator();
            }

            var res = validationResult.Errors.Aggregate(new InfoAccumulator(), (info, f) => info.AddError(f.ErrorMessage));
            return res;
        }

        /// <summary>
        /// Creates the ok response.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        private Response CreateOkResponse(string customerId, string companyId) {
            JObject res = new JObject();
            res.Add("CustomerId", customerId);
            if (!string.IsNullOrEmpty(companyId)) {
                res.Add("CompanyId", companyId);
            }

            return Response.AsJson(res)
                .WithStatusCode(HttpStatusCode.OK);
        }

        /// <summary>
        /// Creates the error response.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="statusCode">The status code.</param>
        /// <returns></returns>
        private Response CreateErrorResponse(string customerId, string companyId, ModelBindingException exception, IEnumerable<string> errors = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest) {
            var response = new ErrorResponseBuilder()
                .AddKeyValue("CustomerId", customerId)
                .AddKeyValue("CompanyId", companyId)
                .AddErrorMessages(errors)
                .AddModelBindingException(exception)
                .BuildResponse();

            return Response.AsJson(response)
                .WithStatusCode(statusCode);
        }
    }
}
