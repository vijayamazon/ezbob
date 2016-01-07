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

    public class CustomerVerificationModule : NancyModule {

        [Injected]
        public CustomerSendVerificationSmsValidator SmsValidator { get; set; }

        [Injected]
        public CustomerValidateVerificationCodeValidator VerificationCodeValidator { get; set; }

        [Injected]
        public CustomerVerificationSmsSendRecieve VerificationSmsSendRecieve { get; set; }

        [Injected]
        public CustomerValidateVerificationCodeSendRecieve ValidateVerificationCodeSendRecieve { get; set; }

        [Injected]
        public RestServerConfig Config { get; set; }

        [Injected]
        public ILog Log { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Nancy.NancyModule"/> class.
        /// </summary>
        public CustomerVerificationModule()
        {
            SendVerificationSms();
            ValidateVerificationCode();
        }

        /// <summary>
        /// Sends the verification SMS.
        /// </summary>
        private void SendVerificationSms() {
            Post["SendVerificationSms", "api/v1/customer/verification/sms/{customerId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                CustomerSendVerificationSmsCommand command;
                try {
                    command = this.Bind<CustomerSendVerificationSmsCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("model binding failed to bind");
                    return CreateErrorResponse(customerId, ex);
                }

                InfoAccumulator info = Validate(command);
                if (info.HasErrors) {
                    return CreateErrorResponse(customerId, null, info.GetErrors());
                }

                //specifying cancellation token makes asynchronous sender to cancel a task after specified timeout
                var cts = new CancellationTokenSource(Config.SendRecieveTaskTimeoutMilis);
                try {
                    var response = await VerificationSmsSendRecieve.SendAsync(Config.ServerAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(customerId, null, response.Errors);
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on send sms message response", ex);
                    return CreateErrorResponse(customerId, null, null, HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(customerId);
            };
        }

        /// <summary>
        /// Validates the verification code.
        /// </summary>
        private void ValidateVerificationCode() {
            Post["ValidateVerificationCode", "api/v1/customer/verification/sms/{customerId}/{verificationToken}", runAsync: true] = async (o, ct) => {
                CustomerValidateVerificationCodeCommand command;
                string customerId = o.customerId;

                //Bind
                try {
                    command = this.Bind<CustomerValidateVerificationCodeCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("model binding failed to bind");
                    return CreateErrorResponse(customerId, ex);
                }

                //Validate
                InfoAccumulator info = Validate(command);
                if (info.HasErrors) {
                    return CreateErrorResponse(customerId, null, info.GetErrors());
                }

                //Send Command
                //specifying cancellation token makes asynchronous sender to cancel a task after specified timeout
                var cts = new CancellationTokenSource(Config.SendRecieveTaskTimeoutMilis);
                try {
                    var response = await ValidateVerificationCodeSendRecieve.SendAsync(Config.ServerAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(customerId, null, response.Errors);
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on verification code validation");
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
        private InfoAccumulator Validate(CustomerSendVerificationSmsCommand command) {
            var validationResult = SmsValidator.Validate(command);
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
        private InfoAccumulator Validate(CustomerValidateVerificationCodeCommand command) {
            var validationResult = VerificationCodeValidator.Validate(command);
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
        private InfoAccumulator CreateInfoAccumulator(ValidationResult validationResult) {
            var res = validationResult.Errors.Aggregate(new InfoAccumulator(), (info, f) => info.AddError(f.ErrorMessage));
            return res;
        }

        /// <summary>
        /// Creates the ok response.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        private Response CreateOkResponse(string customerId) {
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
        private Response CreateErrorResponse(string customerId, ModelBindingException exception, IEnumerable<string> errors = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest) {
            var response = new ErrorResponseBuilder().AddKeyValue("CustomerId", customerId)
                .AddErrorMessages(errors)
                .AddModelBindingException(exception)
                .BuildResponse();

            return Response.AsJson(response)
                .WithStatusCode(statusCode);
        }
    }
}
