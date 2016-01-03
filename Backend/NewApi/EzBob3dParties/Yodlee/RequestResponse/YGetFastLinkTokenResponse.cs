namespace EzBob3dParties.Yodlee.RequestResponse
{
    using System.Collections.Generic;
    using System.Linq;
    using EzBob3dParties.Yodlee.Models;
    using EzBobCommon.Utils;

    class YGetFastLinkTokenResponse : YResponseBase
    {
        public IList<FinappAuthenticationInfo> finappAuthenticationInfos { get; set; }

        public string FastLinkToken {
            get
            {
                if (CollectionUtils.IsNotEmpty(finappAuthenticationInfos)) {
                    var finappAuthenticationInfo = this.finappAuthenticationInfos.First();
                    return finappAuthenticationInfo.token;
                }

                return null;
            }
        }
    }
}
