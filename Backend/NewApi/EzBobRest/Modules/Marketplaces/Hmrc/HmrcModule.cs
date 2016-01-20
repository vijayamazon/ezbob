namespace EzBobRest.Modules.Marketplaces.Hmrc {
    using System.Threading;
    using System.Threading.Tasks;
    using EzBobApi.Commands.Hmrc;
    using EzBobCommon;
    using EzBobRest.Modules.Marketplaces.Hmrc.NSB;
    using EzBobRest.Modules.Marketplaces.Hmrc.Validators;
    using Nancy;
    using Nancy.ModelBinding;

    public class HmrcModule : NancyModuleBase {
        [Injected]
        public HmrcCustomerRegistrationValidator CustomerRegistrationValidator { get; set; }

        [Injected]
        public HmrcRegisterCustomerCommandSendRecieve RegisterCustomerCommandSendRecieve { get; set; }

        public HmrcModule() {
            Post["RegisterHmrc", "api/v1/marketplace/hmrc/register/{customerId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                HmrcRegisterCustomerCommand command;
                //Bind
                try {
                    command = this.Bind<HmrcRegisterCustomerCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("binding error on hmrc registration request: " + customerId, ex);
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(command, CustomerRegistrationValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithErrorMessages(info.GetErrors()));
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendRecieveTaskTimeoutMilis);
                HmrcRegisterCustomerCommandResponse response;
                try {
                    response = await RegisterCustomerCommandSendRecieve.SendAsync(Config.ServiceAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b
                            .WithCustomerId(customerId)
                            .WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on hmrc registration: " + customerId);
                    return CreateErrorResponse(b => b.WithCustomerId(customerId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b.WithCustomerId(customerId));
            };
        }
    }
}
