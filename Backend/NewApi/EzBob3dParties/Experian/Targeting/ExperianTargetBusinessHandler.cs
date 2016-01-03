namespace EzBob3dParties.Experian.Targeting {
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using EzBob3dPartiesApi.Experian;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobModels.ThirdParties.Experian;
    using NServiceBus;

    /// <summary>
    /// Handles Experian business targeting command
    /// </summary>
    public class ExperianTargetBusinessHandler : HandlerBase<ExperianTarget3dPartyBuisnessCommandResponse>, IHandleMessages<ExperianTarget3dPartyBusinessCommand> {

        [Injected]
        public IExperian Experian { get; set; }

        /// <summary>
        /// Handles a command.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <remarks>
        /// This method will be called when a command arrives on the bus and should contain
        ///             the custom logic to execute when the command is received.
        /// </remarks>
        public void Handle(ExperianTarget3dPartyBusinessCommand command) {
            ResultInfoAccomulator<IEnumerable<Experian3dPartyCompanyInfo>> info = Experian.TargetBusiness(command.CompanyName, command.PostCode, command.IsLimited, command.RegNumber.HasValue ? command.RegNumber.GetValue() : "");
            if (info.HasErrors) {
                //TODO : check again
                if (Transaction.Current != null) {
                    Transaction.Current.Rollback();
                    base.RegisterError(info, command);
                }
            } else {
                SendReply(info, command, o => o.CompanyInfos = info.Result.ToArray());
            }
        }
    }
}
