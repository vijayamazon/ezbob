namespace EzBob3dParties.SimplyPostcode
{
    using EzBob3dPartiesApi.SimplyPostcode;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using NServiceBus;

    /// <summary>
    /// Handles <see cref=""/>.
    /// </summary>
    public class SimplyPostcodeGetAddresses3dPartyCommandHandler : HandlerBase<SimplyPostcodeGetAddresses3dPartyCommandResponse>, 
        IHandleMessages<SimplyPostcodeGetAddresses3dPartyCommand> {
        
        [Injected]
        internal ISimplyPostcodeService SimplyPostcodeService { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public async void Handle(SimplyPostcodeGetAddresses3dPartyCommand command) {
            InfoAccumulator info = new InfoAccumulator();
            SimplyPostCodeAddressSearchResponse response = await SimplyPostcodeService.GetAddressesByPostCode(command.PostCode);
            if (StringUtils.IsNotEmpty(response.errorMessage)) {
                
                info.AddError(response.errorMessage);
                SendReply(info, command);
                
                return;
            }

            SendReply(info, command, resp => resp.Addresses = response.Records);
        }
    }
}
