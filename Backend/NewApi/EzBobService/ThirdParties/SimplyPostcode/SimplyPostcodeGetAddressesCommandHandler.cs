namespace EzBobService.ThirdParties.SimplyPostcode {
    using EzBob3dPartiesApi.SimplyPostcode;
    using EzBobApi.Commands.SimplyPostcode;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using NServiceBus;

    /// <summary>
    /// Processes <see cref="SimplyPostcodeGetAddressesCommand"/>.
    /// </summary>
    public class SimplyPostcodeGetAddressesCommandHandler : HandlerBase<SimplyPostcodeGetAddressesCommandResponse>,
        IHandleMessages<SimplyPostcodeGetAddressesCommand>,
        IHandleMessages<SimplyPostcodeGetAddresses3dPartyCommandResponse> {

        [Injected]
        public ThirdPartyServiceConfig Config { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void Handle(SimplyPostcodeGetAddressesCommand command) {
            var commandToSend = new SimplyPostcodeGetAddresses3dPartyCommand {
                PostCode = command.PostCode
            };

            SendCommand(Config.Address, commandToSend, command);
        }


        /// <summary>
        /// Handles the specified response.
        /// </summary>
        /// <param name="response">The response.</param>
        public void Handle(SimplyPostcodeGetAddresses3dPartyCommandResponse response) {
            ReplyToOrigin(response, resp => resp.Addresses = response.Addresses);
        }
    }
}
