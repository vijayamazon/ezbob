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
                        var error = CreateErrorObject(customerId, companyId, info.GetErrors()
                            .ToArray());
                        return Response.AsJson(error)
                            .WithStatusCode(HttpStatusCode.BadRequest);
                    }

                    //binding automatically fills in company id
                    //when company id is not supplied, the automatic default value is "{companyId}"
                    updateCommand.CompanyId = companyId;

                    UpdateCompanyCommandResponse response = UpdateCompany.SendAndBlockUntilReceive(Config.ServiceAddress, updateCommand);
                    if (CollectionUtils.IsNotEmpty(response.Errors)) {
                        var error = CreateErrorObject(customerId, companyId, response.Errors);
                        return Response.AsJson(error)
                            .WithStatusCode(HttpStatusCode.BadRequest);
                    }

                    JObject res = new JObject();
                    res.Add("CustomerId", response.CustomerId);
                    res.Add("CompanyId", response.CompanyId);
                    return Response.AsJson(res)
                        .WithStatusCode(HttpStatusCode.OK);
                } catch (ModelBindingException ex) {
                    var error = CreateErrorObject(customerId, companyId, ex);
                    return Response.AsJson(error)
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
        /// Creates the error object.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private JObject CreateErrorObject(string customerId, string companyId, string[] errors) {
            JObject error = new JObject();

            error.Add("CustomerId", customerId);
            if (StringUtils.IsNotEmpty(companyId)) {
                error.Add("CompanyId", companyId);
            }

            var errorsArr = new JArray();
            error.Add("Errors", errorsArr);

            foreach (var err in errors) {
                errorsArr.Add(err);
            }

            return error;
        }

        /// <summary>
        /// Creates the error object.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        private JObject CreateErrorObject(string customerId, string companyId, ModelBindingException exception) {
            string errorMsg = "Invalid " + ExtractInvalidPropertyName(exception);
            return CreateErrorObject(customerId, companyId, new string[] {
                errorMsg
            });
        }

        /// <summary>
        /// Extracts the name of the invalid property.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        private string ExtractInvalidPropertyName(ModelBindingException exception) {
            if (exception.InnerException != null) {
                var items = exception.InnerException.Message.Split(',')[0].Split('.');
                return items[items.Length - 1];
            }

            return String.Empty;
        }
    }
}
