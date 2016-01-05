using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobRest.Modules {
    using EzBobApi.Commands.Company;
    using EzBobCommon;
    using EzBobCommon.Utils;
    using EzBobRest.NSB;
    using EzBobRest.ResponseHelpers;
    using EzBobRest.Validators;
    using Nancy;
    using Nancy.ModelBinding;
    using Nancy.Security;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Handles company related REST requests
    /// </summary>
    public class CompanyModule : NancyModule {

        [Injected]
        public UpdateCompanySendReceive UpdateCompany { get; set; }

        [Injected]
        public RestServerConfig Config { get; set; }

        [Injected]
        public CompanyUpdateValidator Validator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompanyModule"/> class.
        /// </summary>
        public CompanyModule() {
//            this.RequiresMSOwinAuthentication();
            Post["UpdateCompany", "api/v1/company/update/{customerId}/{companyId}"] = o => {
                string customerId = o.CustomerId;
                //"{companyId}" - is the default value in case when company id is not supplied by request
                string companyId = o.CompanyId != "{companyId}" ? o.CompanyId : null;
                try {
                    var updateCommand = this.Bind<UpdateCompanyCommand>();
                    InfoAccumulator info = Validate(updateCommand);
                    if (info.HasErrors) {
                        var errorResponse = CreateErrorResponse(customerId, companyId, info.GetErrors());

                        return Response.AsJson(errorResponse)
                            .WithStatusCode(HttpStatusCode.BadRequest);
                    }

                    //binding automatically fills in company id
                    //when company id is not supplied, the automatic default value is "{companyId}"
                    updateCommand.CompanyId = companyId;

                    UpdateCompanyCommandResponse response = UpdateCompany.SendAndBlockUntilReceive(Config.ServiceAddress, updateCommand);
                    if (CollectionUtils.IsNotEmpty(response.Errors)) {
                        var errorResponse = CreateErrorResponse(customerId, companyId, response.Errors);

                        return Response.AsJson(errorResponse)
                            .WithStatusCode(HttpStatusCode.BadRequest);
                    }

                    JObject res = new JObject();
                    res.Add("CustomerId", response.CustomerId);
                    res.Add("CompanyId", response.CompanyId);
                    return Response.AsJson(res)
                        .WithStatusCode(HttpStatusCode.OK);
                } catch (ModelBindingException ex) {
                    var errorResponse = CreateErrorResponse(customerId, companyId, null, ex);

                    return Response.AsJson(errorResponse)
                        .WithStatusCode(HttpStatusCode.BadRequest);
                }
            };
        }

        /// <summary>
        /// Validates the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        private InfoAccumulator Validate(UpdateCompanyCommand command)
        {
            var validationResult = Validator.Validate(command);
            if (validationResult.IsValid)
            {
                return new InfoAccumulator();
            }

            var res = validationResult.Errors.Aggregate(new InfoAccumulator(), (info, f) => info.AddError(f.ErrorMessage));
            return res;
        }

        /// <summary>
        /// Creates the error response.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        private JObject CreateErrorResponse(string customerId, string companyId, IEnumerable<string> errors, ModelBindingException exception = null) {
            var errorResponse = new ErrorResponseBuilder()
                       .AddKeyValue("CustomerId", customerId)
                       .AddKeyValue("CompanyId", companyId)
                       .AddErrorMessages(errors)
                       .AddModelBindingException(exception)
                       .BuildResponse();

            return errorResponse;
        }
    }
}
