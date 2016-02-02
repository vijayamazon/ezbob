namespace EzBobRest.Modules.Customer {
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Logging;
    using EzBobApi.Commands.Customer;
    using EzBobCommon;
    using EzBobRest.Modules.Customer.NSB;
    using EzBobRest.Modules.Customer.Validators;
    using Nancy;
    using Nancy.ModelBinding;

    public class CustomerVerificationModule : NancyModuleBase {

        [Injected]
        public CustomerSendVerificationSmsValidator SmsValidator { get; set; }

        [Injected]
        public CustomerValidateVerificationCodeValidator VerificationCodeValidator { get; set; }

        [Injected]
        public CustomerVerificationSmsSendReceive VerificationSmsSendReceive { get; set; }

        [Injected]
        public CustomerValidateVerificationCodeSendReceive ValidateVerificationCodeSendReceive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Nancy.NancyModule"/> class.
        /// </summary>
        public CustomerVerificationModule() {
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
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithModelBindingException(ex));
                }

                InfoAccumulator info = Validate(command, SmsValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithErrorMessages(info.GetErrors()));
                }

                //specifying cancellation token makes asynchronous sender to cancel a task after specified timeout
                var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
                try {
                    var response = await VerificationSmsSendReceive.SendAsync(Config.ServerAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b
                            .WithCustomerId(customerId)
                            .WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on send sms message response", ex);
                    return CreateErrorResponse(b => b.WithCustomerId(customerId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b.WithCustomerId(customerId));
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
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(command, VerificationCodeValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithErrorMessages(info.GetErrors()));
                }

                //Send Command
                //specifying cancellation token makes asynchronous sender to cancel a task after specified timeout
                var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
                try {
                    var response = await ValidateVerificationCodeSendReceive.SendAsync(Config.ServerAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b
                            .WithCustomerId(customerId)
                            .WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on verification code validation");
                    return CreateErrorResponse(b => b.WithCustomerId(customerId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b.WithCustomerId(customerId));
            };
        }
    }
}
