namespace EzBob3dParties.SimplyPostcode {
    using EzBob3dPartiesApi.SimplyPostcode;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using EzBobModels.SimplyPostcode;
    using NServiceBus;

    /// <summary>
    /// Handles <see cref="SimplyPostcodeGetAddressDetails3dPartyCommand"/>.
    /// </summary>
    public class SimplyPostcodeGetAddressDetails3dPartyCommandHandler :
        HandlerBase<SimplyPostcodeGetAddressDetails3dPartyCommandResponse>,
        IHandleMessages<SimplyPostcodeGetAddressDetails3dPartyCommand> {

        [Injected]
        internal ISimplyPostcodeService SimplyPostcodeService { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public async void Handle(SimplyPostcodeGetAddressDetails3dPartyCommand command) {
            InfoAccumulator info = new InfoAccumulator();
            SimplyPostcodeDatailedAddress address = await SimplyPostcodeService.GetAddressDetails(command.AddressId);

            if (StringUtils.IsNotEmpty(address.Errormessage)) {
                SendReply(info, command);
                return;
            }

            SendReply(info, command, resp => resp.Address = address);
        }
    }
}
