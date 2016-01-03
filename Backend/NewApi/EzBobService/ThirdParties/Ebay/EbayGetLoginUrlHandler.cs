namespace EzBobService.ThirdParties.Ebay {
    using EzBob3dPartiesApi.EBay;
    using EzBobApi.Commands.Ebay;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using NServiceBus;

    /// <summary>
    /// Handles requests and responses about ebay login url
    /// </summary>
    public class EbayGetLoginUrlHandler : HandlerBase<EbayGetLoginUrlCommandResponse>, IHandleMessages<EbayGetLoginUrlCommand>, IHandleMessages<EbayGetLoginUrl3dPartyCommandResponse> {

        [Injected]
        public ThirdPartyServiceConfig ThirdPartyService { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="handledCommand">The command.</param>
        public void Handle(EbayGetLoginUrlCommand handledCommand) {
            SendCommand(ThirdPartyService.Address, new EbayGetLoginUrl3dPartyCommand(), handledCommand);
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="handledCommand">The message.</param>
        public void Handle(EbayGetLoginUrl3dPartyCommandResponse handledCommand) {
            InfoAccumulator info = new InfoAccumulator();
            SendReply(info, handledCommand, resp => {
                resp.EbayLoginUrl = handledCommand.Url;
                resp.SessionId = handledCommand.SessionId;
            });
        }
    }
}
