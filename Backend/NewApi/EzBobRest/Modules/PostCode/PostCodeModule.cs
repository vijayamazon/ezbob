using System.Threading.Tasks;

namespace EzBobRest.Modules.PostCode {
    using System.Threading;
    using EzBobApi.Commands.SimplyPostcode;
    using EzBobCommon;
    using EzBobRest.Modules.PostCode.NSB;
    using EzBobRest.Modules.PostCode.Validators;
    using Nancy;
    using Nancy.ModelBinding;

    public class PostCodeModule : NancyModuleBase {
        [Injected]
        public GetAddressesByPostCodeValidator GetAddressesByPostCodeValidator { get; set; }

        [Injected]
        public GetAddressDetailsValidator GetAddressDetailsValidator { get; set; }

        [Injected]
        public GetAddressesByPostcodeSendReceive GetAddressesByPostcodeSendReceive { get; set; }

        [Injected]
        public GetAddressDetailsSendReceive GetAddressDetailsSendReceive { get; set; }

        public PostCodeModule() {
            GetAddressesByPostcode();
            GetAddressDetails();
        }

        /// <summary>
        /// Gets the addresses by postcode.
        /// </summary>
        private void GetAddressesByPostcode() {
            Get["GetAddressesByPostcode", "api/v1/address/{postcode}/{customerId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                SimplyPostcodeGetAddressesCommand command;
                //Bind
                try {
                    command = this.Bind<SimplyPostcodeGetAddressesCommand>();
                } catch (ModelBindingException ex) {
                    Log.Warn("binding error on get addresses by postcode request: " + customerId, ex);
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(command, GetAddressesByPostCodeValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithErrorMessages(info.GetErrors()));
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
                SimplyPostcodeGetAddressesCommandResponse response;
                try {
                    response = await GetAddressesByPostcodeSendReceive.SendAsync(Config.ServiceAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b
                            .WithCustomerId(customerId)
                            .WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on get address by post code request: " + customerId);
                    return CreateErrorResponse(b => b.WithCustomerId(customerId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b
                    .WithCustomerId(customerId)
                    .WithKeyValue(() => response.Addresses.Count)
                    .WithKeyValue(() => response.Addresses));
            };
        }

        /// <summary>
        /// Gets the address details.
        /// </summary>
        private void GetAddressDetails() {
            Get["GetAddressDetails", "api/v1/address/{addressId}/{customerId}", runAsync: true] = async (o, ct) => {
                string customerId = o.customerId;
                SimplyPostcodeGetAddressDetailsCommand command;
                //Bind
                try {
                    command = this.Bind<SimplyPostcodeGetAddressDetailsCommand>();
                } catch (ModelBindingException ex) {
                    return CreateErrorResponse(b =>
                        b.WithCustomerId(customerId)
                            .WithModelBindingException(ex));
                }

                //Validate
                InfoAccumulator info = Validate(command, GetAddressDetailsValidator);
                if (info.HasErrors) {
                    return CreateErrorResponse(b => b
                        .WithCustomerId(customerId)
                        .WithErrorMessages(info.GetErrors()));
                }

                //Send Command
                var cts = new CancellationTokenSource(Config.SendReceiveTaskTimeoutMilis);
                SimplyPostcodeGetAddressDetailsCommandResponse response;
                try {
                    response = await GetAddressDetailsSendReceive.SendAsync(Config.ServiceAddress, command, cts);
                    if (response.HasErrors) {
                        return CreateErrorResponse(b => b
                            .WithCustomerId(customerId)
                            .WithErrorMessages(response.Errors));
                    }
                } catch (TaskCanceledException ex) {
                    Log.Error("timeout on get address details request: " + customerId);
                    return CreateErrorResponse(b => b.WithCustomerId(customerId), HttpStatusCode.InternalServerError);
                }

                return CreateOkResponse(b => b
                    .WithCustomerId(customerId)
                    .WithKeyValue(() => response.Address));
            };
        }
    }
}