using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.EBay {
    using EzBob3dPartiesApi.EBay;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using NServiceBus;

    /// <summary>
    /// Handles GetSignInUrl3dPartyCommand
    /// </summary>
    public class EBaySignInCommandHandler : HandlerBase<EbayGetLoginUrl3dPartyCommandResponse>, IHandleMessages<EbayGetLoginUrl3dPartyCommand> {

        /// <summary>
        /// Gets or sets the e-bay service.
        /// </summary>
        /// <value>
        /// The e-bay service.
        /// </value>
        [Injected]
        public EBayService EBayService { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public async void Handle(EbayGetLoginUrl3dPartyCommand command) {
            string sessionId = await EBayService.GetSessionId();
            string url = EBayService.GetLoginUrl(sessionId);
            
            InfoAccumulator info = new InfoAccumulator();
            SendReply(info, command, resp => {
                resp.Url = url;
                resp.SessionId = sessionId;
            });
        }
    }
}
