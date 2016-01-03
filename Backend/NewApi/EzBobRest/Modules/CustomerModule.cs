namespace EzBobRest.Modules {
    using System;
    using System.Linq;
    using EzBobApi.Commands.Customer;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using EzBobRest.NSB;
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

                } catch (Exception ex) {
                    InfoAccumulator info = new InfoAccumulator();
                    info.AddError("Invalid request format");
                    return Response.AsJson(info)
                        .WithStatusCode(HttpStatusCode.BadRequest);
                }
            };
        }

        /// <summary>
        /// Customers the update.
        /// </summary>
        private void CustomerUpdate() {
            Post["UpdateCustomer", "api/v1/customer/update/{id}"] = o => {
                string id = o.id;
                CustomerUpdateCommand updateCommand = null;
                try {
                    updateCommand = this.Bind<CustomerUpdateCommand>();
                } catch (ModelBindingException ex) {
                    JObject resp = new JObject();
                    resp.Add("CustomerId", id);
                    var errors = new JArray();
                    resp.Add("Errors", errors);
                    string errorMsg = "Invalid " + ExtractInvalidPropertyName(ex);
                    errors.Add(errorMsg);
                    return Response.AsJson(resp)
                        .WithStatusCode(HttpStatusCode.BadRequest);
                }

                if (updateCommand != null) {
                    updateCommand.CustomerId = id;
                    var response = UpdateSender.SendAndBlockUntilReceive(Config.ServiceAddress, updateCommand);
                    return CreateResponse(response);
                }

                return null;
            };
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
        private Response CreateResponse(dynamic response) {
            if (CollectionUtils.IsNotEmpty(response.Errors)) {
                JObject errors = new JObject {
                    {
                        "CustomerId", response.CustomerId
                    }, {
                        "Errors", new JArray(response.Errors)
                    }
                };

                return Response.AsJson(errors)
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
    }
}
