namespace EzBob3dParties.Yodlee.RequestResponse {
    using System.Collections.Generic;
    using EzBob3dParties.Yodlee.Models.SiteAccount;

    /// <summary>
    /// Response YSiteAccountsRequest
    /// </summary>
    internal class YSiteAccountsResponse : YResponseBase {
        public YSiteAccountsResponse(IList<SiteAccountInfo> infos) {
            AccountInfos = infos;
        }

        /// <summary>
        /// Gets the account infos.
        /// </summary>
        /// <value>
        /// The account infos.
        /// </value>
        public IList<SiteAccountInfo> AccountInfos { get; private set; }
    }
}
