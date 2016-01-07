namespace EzBobRest.Modules {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Logging;
    using EzBobApi.Commands.Company;
    using EzBobApi.Commands.Experian;
    using EzBobCommon;
    using EzBobRest.NSB;
    using EzBobRest.ResponseHelpers;
    using EzBobRest.Validators;
    using FluentValidation.Results;
    using Nancy;
    using Nancy.ModelBinding;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Handles company related REST requests
    /// </summary>
    public class CompanyModule : NancyModule {

        [Injected]
        public UpdateCompanySendReceive UpdateCompanySendReceive { get; set; }

        [Injected]
        public TargetCompanySendReceive TargetCompanySendReceive { get; set; }

        [Injected]
        public RestServerConfig Config { get; set; }

        [Injected]
        public CompanyUpdateValidator UpdateValidator { get; set; }

        [Injected]
        public CompanyTargetingValidator TargetingValidator { get; set; }

        [Injected]
        public ILog Log { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompanyModule"/> class.
        /// </summary>
        public CompanyModule() {
//            this.RequiresMSOwinAuthentication();
            UpdateCompany();
            TargetCompany();
        }

        private void TargetCompany() {
            Post["TargetCompany", "api/v1/company/target/{customerId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                ExperianBusinessTargetingCommand command;
                //Bind
                try {
                    command = this.Bind<ExperianBusinessTargetingCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("binding error on target company", ex);
                    return CreateErrorResponse(customerId, null, ex);
                }

                //Validate
                InfoAccumulator info = Validate(command);
                if (info.HasErrors) {
                    return CreateErrorResponse(customerId, null, null, info.GetErrors());
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendRecieveTaskTimeoutMilis);
                ExperianBusinessTargetingCommandResponse response;
                try {
                    response = await TargetCompanySendReceive.SendAsync(Config.ServerAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(customerId, null, null, info.GetErrors());
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on target company");
                    return CreateErrorResponse(customerId, null, null, null, HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(customerId, response.CompanyInfos);
            };
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
                    Log.Warn("binding error on update company", ex);
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
            var validationResult = UpdateValidator.Validate(command);
            if (validationResult.IsValid) {
                return new InfoAccumulator();
            }

            return AggregateValidationErrors(validationResult);
        }

        /// <summary>
        /// Validates the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        private InfoAccumulator Validate(ExperianBusinessTargetingCommand command) {
            var validationResult = TargetingValidator.Validate(command);
            if (validationResult.IsValid) {
                return new InfoAccumulator();
            }

            return AggregateValidationErrors(validationResult);
        }

        /// <summary>
        /// Aggregates the validation errors.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <returns></returns>
        private static InfoAccumulator AggregateValidationErrors(ValidationResult validationResult)
        {
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
        /// Creates the ok response.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="companyInfos">The company infos.</param>
        /// <returns></returns>
        private Response CreateOkResponse(string customerId, ExperianCompanyInfo[] companyInfos) {
            return Response.AsJson(new {
                CustomerId = customerId,
                Suggestions = companyInfos
            })
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
