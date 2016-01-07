namespace EzBobRest.Modules {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Logging;
    using EzBobApi.Commands.Customer;
    using EzBobCommon;
    using EzBobRest.NSB;
    using EzBobRest.ResponseHelpers;
    using EzBobRest.Validators;
    using FluentValidation.Results;
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
        public CustomerUpdateValidator UpdateValidator { get; set; }

        [Injected]
        public SignupCommandSendReceive SignupSendReceive { get; set; }

        [Injected]
        public UpdateCustomerSendReceive UpdateSendReceive { get; set; }

        [Injected]
        public RestServerConfig Config { get; set; }

        [Injected]
        public ILog Log { get; set; }

        public CustomerModule() {
//            this.RequiresMSOwinAuthentication();
            CustomerSignup();
            CustomerUpdate();
        }

        /// <summary>
        /// Customers the signup.
        /// </summary>
        private void CustomerSignup() {
            Post["SignupCustomer", "api/v1/customer/signup", runAsync: true] = async (o, ct) => {
                CustomerSignupCommand signupCommand;
                //Bind
                try {
                    signupCommand = this.Bind<CustomerSignupCommand>();
                } catch (ModelBindingException ex) {
                    return CreateErrorResponse(null, ex);
                }

                //Validate
                signupCommand.Account.RemoteIp = Context.Request.UserHostAddress;
                InfoAccumulator info = Validate(signupCommand);
                if (!info.HasErrors) {
                    return CreateErrorResponse(null, null, info.GetErrors());
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendRecieveTaskTimeoutMilis);
                CustomerSignupCommandResponse response;
                try {
                    response = await SignupSendReceive.SendAsync(Config.ServerAddress, signupCommand, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(null, null, response.Errors);
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on signup");
                    return CreateErrorResponse(null, null, null, HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(response.CustomerId);
            };
        }

        /// <summary>
        /// Updates customer.
        /// </summary>
        private void CustomerUpdate() {
            Post["UpdateCustomer", "api/v1/customer/update/{customerId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                CustomerUpdateCommand updateCommand;
                //Bind
                try {
                    updateCommand = this.Bind<CustomerUpdateCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("model binding failed to bind");
                    return CreateErrorResponse(customerId, ex);
                }

                //Validate
                InfoAccumulator info = Validate(updateCommand);
                if (info.HasErrors) {
                    return CreateErrorResponse(customerId, null, info.GetErrors());
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendRecieveTaskTimeoutMilis);
                try {
                    var response = await UpdateSendReceive.SendAsync(Config.ServiceAddress, updateCommand, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(customerId, null, response.Errors);
                    }

                } catch (TaskCanceledException ex) {
                    Log.Error("time out on update customer");
                    return CreateErrorResponse(customerId, null, null, HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(customerId);
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

            return CreateInfoAccumulator(validationResult);
        }


        /// <summary>
        /// Validates the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        private InfoAccumulator Validate(CustomerUpdateCommand command) {
            var validationResult = UpdateValidator.Validate(command);
            if (validationResult.IsValid) {
                return new InfoAccumulator();
            }
            return CreateInfoAccumulator(validationResult);
        }

        /// <summary>
        /// Creates the information accumulator.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <returns></returns>
        private static InfoAccumulator CreateInfoAccumulator(ValidationResult validationResult) {
            var res = validationResult.Errors.Aggregate(new InfoAccumulator(), (info, f) => info.AddError(f.ErrorMessage));
            return res;
        }

        /// <summary>
        /// Creates the ok response.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        private Response CreateOkResponse(string customerId)
        {
            JObject res = new JObject();
            res.Add("CustomerId", customerId);
            return Response.AsJson(res)
                .WithStatusCode(HttpStatusCode.OK);
        }

        /// <summary>
        /// Creates the error response.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="statusCode">The status code.</param>
        /// <returns></returns>
        private Response CreateErrorResponse(string customerId, ModelBindingException exception, IEnumerable<string> errors = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            var response = new ErrorResponseBuilder()
                .AddKeyValue("CustomerId", customerId)
                .AddErrorMessages(errors)
                .AddModelBindingException(exception)
                .BuildResponse();

            return Response.AsJson(response)
                .WithStatusCode(statusCode);
        }
    }
}
