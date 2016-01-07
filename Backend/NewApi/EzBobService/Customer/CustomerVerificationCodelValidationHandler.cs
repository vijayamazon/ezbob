using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobService.Customer {
    using EzBobApi.Commands.Customer;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobService.Misc;
    using NServiceBus;

    /// <summary>
    /// Handles verification request
    /// </summary>
    public class CustomerVerificationCodelValidationHandler : HandlerBase<CustomerValidateVerificationCodeCommandResponse>, IHandleMessages<CustomerValidateVerificationCodeCommand> {

        [Injected]
        public VerificationHelper VerificationHelper { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void Handle(CustomerValidateVerificationCodeCommand command) {
            InfoAccumulator info = new InfoAccumulator();
            bool isValid = VerificationHelper.Validate(command.VerificationCode, command.VerificationToken);
            if (!isValid) {
                Log.Info("Invalid verification code");
                info.AddError("Invalid verification code");
            }
            SendReply(info, command, resp => resp.CustomerId = command.CustomerId);

        }
    }
}
