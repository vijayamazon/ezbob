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
        public EbayRegisterCustomerSendRecieve RegisterCustomerSendRecieve { get; set; }

        [Injected]
        public EbayGetRedirectUrlSendRecieve GetRedirectUrlSendRecieve { get; set; }


        public EbayModule() {
            GetRedirectUrl();
            RegisterCustomer();
        }

        private void GetRedirectUrl() {
            Post["EbayRirectUrl", "api/v1/marketplace/ebay/redirectUrl/{customerId}", runAsync: true] = async (o, ct) => {
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
                var cts = new CancellationTokenSource(Config.SendRecieveTaskTimeoutMilis);
                EbayGetLoginUrlCommandResponse response;
                try {
                    response = await GetRedirectUrlSendRecieve.SendAsync(Config.ServiceAddress, command, cts);
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
                var cts = new CancellationTokenSource(Config.SendRecieveTaskTimeoutMilis);
                EbayRegisterCustomerCommandResponse response;
                try {
                    response = await RegisterCustomerSendRecieve.SendAsync(Config.ServiceAddress, command, cts);
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
