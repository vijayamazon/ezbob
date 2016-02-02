namespace EzBobRest.Modules.Marketplaces.Ebay {
    using System.Threading;
    using System.Threading.Tasks;
    using EzBobApi.Commands.Ebay;
    using EzBobCommon;
    using EzBobRest.Modules.Marketplaces.Ebay.NSB;
    using EzBobRest.Modules.Marketplaces.Ebay.Validators;
    using Nancy;
    using Nancy.ModelBinding;

    public class EbayModule : NancyModuleBase {
        
        [Injected]
        public EbayGetRedirectUrlValidator GetRedirectUrlValidator { get; set; }

        [Injected]
        public EbayRegisterCustomerValidator RegisterCustomerValidator { get; set; }

        [Injected]
        public EbayRegisterCustomerSendReceive RegisterCustomerSendReceive { get; set; }

        [Injected]
        public EbayGetRedirectUrlSendReceive GetRedirectUrlSendReceive { get; set; }


        public EbayModule() {
            GetRedirectUrl();
            RegisterCustomer();
        }

        /// <summary>
        /// Gets the redirect URL.
        /// </summary>
        private void GetRedirectUrl() {
            Get["EbayRirectUrl", "api/v1/marketplace/ebay/redirectUrl/{customerId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                EbayGetLoginUrlCommand command;
                //Bind
                try {
                    command = this.Bind<EbayGetLoginUrlCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("binding error on ebay redirect url request: " + customerId, ex);
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(command, GetRedirectUrlValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithErrorMessages(info.GetErrors()));
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
                EbayGetLoginUrlCommandResponse response;
                try {
                    response = await GetRedirectUrlSendReceive.SendAsync(Config.ServiceAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b.WithCustomerId(customerId)
                            .WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on ebay get redirect url: " + customerId);
                    return CreateErrorResponse(b => b.WithCustomerId(customerId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b
                    .WithCustomerId(customerId)
                    .WithRedirectUrl(response.EbayLoginUrl)
                    .WithSessionId(response.SessionId));
            };
        }

        /// <summary>
        /// Registers the customer.
        /// </summary>
        private void RegisterCustomer() {
            Post["EbayRegisterCustomer", "api/v1/marketplace/ebay/register/{customerId}/{sessionId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                EbayRegisterCustomerCommand command;

                //Bind
                try {
                    command = this.Bind<EbayRegisterCustomerCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("Binding error on ebay register customer request: " + customerId, ex);
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
                EbayRegisterCustomerCommandResponse response;
                try {
                    response = await RegisterCustomerSendReceive.SendAsync(Config.ServiceAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b
                            .WithCustomerId(customerId)
                            .WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on register ebay customer: " + customerId);
                    return CreateErrorResponse(b => b.WithCustomerId(customerId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b
                    .WithCustomerId(customerId));
            };
        }
    }
}
