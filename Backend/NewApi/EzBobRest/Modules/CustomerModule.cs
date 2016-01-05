namespace EzBobRest.Modules {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EzBobApi.Commands.Customer;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using EzBobRest.NSB;
    using EzBobRest.ResponseHelpers;
    using EzBobRest.Validators;
    using Nancy;
    using Nancy.ModelBinding;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Handles customer REST requests
    /// </summary>
    public class CustomerModule : NancyModule {

        [Injected]
        public CustomerSignupValidator SignupValidator { get; set; }

        [Injected]
        public SignupCommandSendReceive SignupSender { get; set; }

        [Injected]
        public UpdateCustomerSendReceive UpdateSender { get; set; }

        [Injected]
        public RestServerConfig Config { get; set; }

        public CustomerModule() {
//            this.RequiresMSOwinAuthentication();
            CustomerSignup();
            CustomerUpdate();
        }

        /// <summary>
        /// Customers the signup.
        /// </summary>
        private void CustomerSignup() {
            Post["SignupCustomer", "api/v1/customer/signup"] = o => {
                try {
                    var signupCommand = this.Bind<CustomerSignupCommand>();
                    signupCommand.Account.RemoteIp = Context.Request.UserHostAddress;
                    InfoAccumulator info = Validate(signupCommand);
                    if (!info.HasErrors) {
                        var response = SignupSender.SendAndBlockUntilReceive(Config.ServiceAddress, signupCommand);
                        return CreateResponse(response);
                    }

                    return Response.AsJson(info)
                        .WithStatusCode(HttpStatusCode.BadRequest);

                } catch (ModelBindingException ex) {
                    var errorRespone = CreateErrorResponse(null, null, ex);
                    return Response.AsJson(errorRespone)
                        .WithStatusCode(HttpStatusCode.BadRequest);
                }
            };
        }

        /// <summary>
        /// Customers the update.
        /// </summary>
        private void CustomerUpdate() {
            Post["UpdateCustomer", "api/v1/customer/update/{id}"] = o => {
                string customerId = o.id;
                CustomerUpdateCommand updateCommand = null;
                try {
                    updateCommand = this.Bind<CustomerUpdateCommand>();
                } catch (ModelBindingException ex) {
                    var errorRespone = CreateErrorResponse(customerId, null, ex);
                    return Response.AsJson(errorRespone)
                        .WithStatusCode(HttpStatusCode.BadRequest);
                }

                if (updateCommand != null) {
                    updateCommand.CustomerId = customerId;
                    var response = UpdateSender.SendAndBlockUntilReceive(Config.ServiceAddress, updateCommand);
                    return CreateResponse(response);
                }

                return null;
            };
        }

        /// <summary>
        /// Validates the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        private InfoAccumulator Validate(CustomerSignupCommand command) {
            var validationResult = SignupValidator.Validate(command);
            if (validationResult.IsValid) {
                return new InfoAccumulator();
            }

            var res = validationResult.Errors.Aggregate(new InfoAccumulator(), (info, f) => info.AddError(f.ErrorMessage));
            return res;
        }

        /// <summary>
        /// Creates the response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        private Response CreateResponse(CustomerCommandResponseBase response) {
            if (CollectionUtils.IsNotEmpty(response.Errors)) {

                var errorResponse = new ErrorResponseBuilder().AddKeyValue("CustomerId", response.CustomerId)
                    .AddErrorMessages(response.Errors)
                    .BuildResponse();

                return Response.AsJson(errorResponse)
                    .WithStatusCode(HttpStatusCode.BadRequest);
            }

            JObject resp = new JObject {
                {
                    "CustomerId", response.CustomerId
                }
            };
            return Response.AsJson(resp)
                .WithStatusCode(HttpStatusCode.OK);
        }

        /// <summary>
        /// Creates the error response.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        private JObject CreateErrorResponse(string customerId, IEnumerable<string> errors, ModelBindingException exception = null)
        {
            var errorResponse = new ErrorResponseBuilder()
                       .AddKeyValue("CustomerId", customerId)
                       .AddErrorMessages(errors)
                       .AddModelBindingException(exception)
                       .BuildResponse();

            return errorResponse;
        }
    }
}
