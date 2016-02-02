using System.Threading.Tasks;

namespace EzBobRest.Modules.Marketplaces.Amazon {
    using System.Threading;
    using EzBobApi.Commands.Amazon;
    using EzBobCommon;
    using EzBobRest.Modules.Marketplaces.Amazon.NSB;
    using EzBobRest.Modules.Marketplaces.Amazon.Validators;
    using Nancy;
    using Nancy.ModelBinding;

    /// <summary>
    /// Contains amazon related api
    /// </summary>
    public class AmazonModule : NancyModuleBase {
        [Injected]
        public AmazonRegisterCustomerCommandValidator RegisterCustomerValidator { get; set; }

        [Injected]
        public AmazonRegisterCustomerSendReceive RegisterCustomerSendReceive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Nancy.NancyModule"/> class.
        /// </summary>
        public AmazonModule() {
            Post["AmazonRegisterCustomer", "api/v1/marketplace/amazon/register/{customerId}", runAsync: true] = async (o, ct) => {
                //Bind
                string customerId = o.customerId;
                AmazonRegisterCustomerCommand command;

                try {
                    command = this.Bind<AmazonRegisterCustomerCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("binding error on register amazon customer request: " + customerId, ex);
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
                AmazonRegisterCustomerCommandResponse response;
                try {
                    response = await RegisterCustomerSendReceive.SendAsync(Config.ServiceAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b
                            .WithCustomerId(customerId)
                            .WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on register amazon customer: " + customerId);
                    return CreateErrorResponse(b => b.WithCustomerId(customerId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b
                    .WithCustomerId(customerId));
            };
        }
    }
}
