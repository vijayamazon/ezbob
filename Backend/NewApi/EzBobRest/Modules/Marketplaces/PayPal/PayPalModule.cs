namespace EzBobRest.Modules.Marketplaces.PayPal {
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Logging;
    using EzBobApi.Commands.PayPal;
    using EzBobCommon;
    using EzBobRest.Modules.Marketplaces.PayPal.NSB;
    using EzBobRest.Modules.Marketplaces.PayPal.Validators;
    using Nancy;
    using Nancy.ModelBinding;

    public class PayPalModule : NancyModuleBase {

        [Injected]
        public PayPalGetRedirectionUrlSendReceive GetRedirectionUrlSendReceive { get; set; }

        [Injected]
        public PayPalRegisterCustomerSendReceive RegisterCustomerSendReceive { get; set; }

        [Injected]
        public PayPalGetRedirectUrlValidator PayPalGetRedirectUrlValidator { get; set; }

        [Injected]
        public PayPalRegisterCustomerValidator RegisterCustomerValidator { get; set; }

        public PayPalModule() {
            PayPalRedirectUrl();
            PayPalRegisterCustomer();
        }

        /// <summary>
        /// Pay-pal register customer.
        /// </summary>
        private void PayPalRegisterCustomer() {
            Post["PayPalRegisterCustomer", "api/v1/marketplace/paypal/register/{customerId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                PayPalRegisterCustomerCommand command;

                //Bind
                try {
                    command = this.Bind<PayPalRegisterCustomerCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("Binding error on pypal register customer request: " + customerId, ex);
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(command, RegisterCustomerValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithErrorMessages(info.GetErrors()));
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
                PayPalRegisterCustomerCommandResponse response;
                try {
                    response = await RegisterCustomerSendReceive.SendAsync(Config.ServiceAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b
                            .WithCustomerId(customerId)
                            .WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on register paypal customer: " + customerId);
                    return CreateErrorResponse(b => b.WithCustomerId(customerId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b
                    .WithCustomerId(customerId));
            };
        }

        /// <summary>
        /// Pay-pal redirect URL.
        /// </summary>
        private void PayPalRedirectUrl() {
            Get["PayPalRirectUrl", "api/v1/marketplace/paypal/redirectUrl/{customerId}/{callback}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                PayPalGetPermissionsRedirectUrlCommand command;
                //Bind
                try {
                    command = this.Bind<PayPalGetPermissionsRedirectUrlCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("binding error on paypal redirect url request: " + customerId, ex);
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(command, PayPalGetRedirectUrlValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithErrorMessages(info.GetErrors()));
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
                PayPalGetPermissionsRedirectUrlCommandResponse response;
                try {
                    response = await GetRedirectionUrlSendReceive.SendAsync(Config.ServiceAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b.WithCustomerId(customerId)
                            .WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on paypal get redirect url: " + customerId);
                    return CreateErrorResponse(b => b.WithCustomerId(customerId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b
                    .WithCustomerId(customerId)
                    .WithRedirectUrl(response.PermissionsRedirectUrl));
            };
        }
    }
}
