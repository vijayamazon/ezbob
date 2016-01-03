namespace EzBobService.ThirdParties.Experian {
    using System.Linq;
    using EzBob3dPartiesApi.Experian;
    using EzBobApi.Commands.Experian;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobModels.ThirdParties.Experian;
    using NServiceBus;

    /// <summary>
    /// Handles business targeting request to experian
    /// </summary>
    public class ExperianBusinessTargetingHandler : HandlerBase<ExperianBusinessTargetingCommandResponse>,
        IHandleMessages<ExperianBusinessTargetingCommand>,
        IHandleMessages<ExperianTarget3dPartyBuisnessCommandResponse> {

        [Injected]
        public ThirdPartyServiceConfig ThirdPartyService { get; set; }

        /// <summary>
        /// Handles message from client and send request to 3d party service.
        /// </summary>
        /// <param name="handledCommand">The command.</param>
        public void Handle(ExperianBusinessTargetingCommand handledCommand) {

            ExperianTarget3dPartyBusinessCommand cmd = new ExperianTarget3dPartyBusinessCommand
            {
                RegNumber = string.IsNullOrEmpty(handledCommand.RegistrationNumber) ? Optional<string>.Empty() : Optional<string>.Of(handledCommand.RegistrationNumber),
                CompanyName = handledCommand.CompanyName,
                IsLimited = handledCommand.IsLimited,
                PostCode = handledCommand.PostCode
            };

            SendCommand(ThirdPartyService.Address, cmd, handledCommand);
        }


        /// <summary>
        /// Handles a 3d party response.
        /// </summary>
        /// <param name="handledResponse">The command.</param>
        public void Handle(ExperianTarget3dPartyBuisnessCommandResponse handledResponse) {
            ReplyToOrigin(handledResponse, resp => {
                if (handledResponse.CompanyInfos != null) {
                    resp.CompanyInfos = handledResponse.CompanyInfos.Select(Transform)
                        .ToArray();
                }
            });
        }

        /// <summary>
        /// Transforms the specified company information.
        /// </summary>
        /// <param name="companyInfo">The company information.</param>
        /// <returns></returns>
        private ExperianCompanyInfo Transform(Experian3dPartyCompanyInfo companyInfo) {
            return new ExperianCompanyInfo {
                PostCode = companyInfo.PostCode,
                AddrLine1 = companyInfo.AddrLine1,
                AddrLine2 = companyInfo.AddrLine2,
                AddrLine3 = companyInfo.AddrLine3,
                AddrLine4 = companyInfo.AddrLine4,
                BusName = companyInfo.BusName,
                BusRefNum = companyInfo.BusRefNum,
                BusinessStatus = companyInfo.BusinessStatus,
                LegalStatus = companyInfo.LegalStatus,
                MatchScore = companyInfo.MatchScore,
                MatchedBusName = companyInfo.MatchedBusName,
                MatchedBusNameType = companyInfo.MatchedBusNameType,
                SicCode = companyInfo.SicCode,
                SicCodeDesc = companyInfo.SicCodeDesc,
                SicCodeType = companyInfo.SicCodeType
            };
        }
    }
}
