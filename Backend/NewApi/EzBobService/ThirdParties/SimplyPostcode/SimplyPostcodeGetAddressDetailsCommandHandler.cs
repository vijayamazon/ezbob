namespace EzBobService.ThirdParties.SimplyPostcode {
    using EzBob3dPartiesApi.SimplyPostcode;
    using EzBobApi.Commands.SimplyPostcode;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using NServiceBus;

    /// <summary>
    /// Processes <see cref="SimplyPostcodeGetAddressDetailsCommand"/>.
    /// </summary>
    public class SimplyPostcodeGetAddressDetailsCommandHandler : HandlerBase<SimplyPostcodeGetAddressDetailsCommandResponse>,
        IHandleMessages<SimplyPostcodeGetAddressDetailsCommand>,
        IHandleMessages<SimplyPostcodeGetAddressDetails3dPartyCommandResponse> {

        [Injected]
        public ThirdPartyServiceConfig Config { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void Handle(SimplyPostcodeGetAddressDetailsCommand command) {
            var commandToSend = new SimplyPostcodeGetAddressDetails3dPartyCommand {
                AddressId = command.AddressId
            };

            SendCommand(Config.Address, commandToSend, command);
        }

        /// <summary>
        /// Handles the specified response.
        /// </summary>
        /// <param name="response">The response.</param>
        public void Handle(SimplyPostcodeGetAddressDetails3dPartyCommandResponse response) {
            ReplyToOrigin(response, resp => resp.Address = response.Address);
        }
    }
}
