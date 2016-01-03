using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.EBay {
    using EzBob3dPartiesApi.EBay;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using NServiceBus;

    /// <summary>
    /// Handles validation requests
    /// </summary>
    public class EbayValidationCommandHandler : HandlerBase<EbayValidationCommandResponse>, IHandleMessages<EbayValidationCommand> {

        [Injected]
        public EBayService EBayService { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public async void Handle(EbayValidationCommand command) {
            InfoAccumulator info = new InfoAccumulator();
            if (string.IsNullOrEmpty(command.Token)) {
                string msg = "got empty token";
                info.AddError(msg);
                Log.Error(msg);
                SendReply(info, command); //send reply with error
                return;
            }

            if (command.IsValidateUserAccount.IsTrue()) {
                bool isAccountValid = await EBayService.ValidateAccount(command.Token);
                SendReply(info, command, resp => {
                    resp.IsAccountValid = isAccountValid;
                    resp.Token = command.Token;
                    resp.Payload = command.PayLoad; //returns payload
                });
            }
        }
    }
}
