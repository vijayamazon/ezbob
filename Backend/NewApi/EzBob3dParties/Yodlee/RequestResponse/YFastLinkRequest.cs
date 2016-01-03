using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee.RequestResponse {
    internal class YFastLinkRequest : YRequestBase {

        private static readonly string formHtmlContent = "<div class='center processText'>Processing...</div>"
            + "<div>"
            + "<form action='{0}' method='post' id='rsessionPost'>"
            + "	RSession : <input type='text' name='rsession' placeholder='rsession' value='{1}' id='rsession'/><br/>"
            + "	FinappId : <input type='text' name='app' placeholder='FinappId' value='{2}' id='finappId'/><br/>"
            + "	Redirect : <input type='text' name='redirectReq' placeholder='true/false' value='true'/><br/>"
            + "	Token : <input type='text' name='token' placeholder='token' value='{3}' id='token'/><br/>"
            + "	Extra Params : <input type='text' name='extraParams' placeholer='Extra Params' value='{4}' id='extraParams'/><br/>"
            + "</form></div><script>document.getElementById('rsessionPost').submit();</script>";

        /// <summary>
        /// Sets the user session token.
        /// </summary>
        /// <param name="userToken">The user token.</param>
        /// <returns></returns>
        public YFastLinkRequest SetUserSessionToken(string userToken) {
            Insert(Rsession, userToken);
            Insert("app", Fastlink2AggregationAppID);
            Insert("redirectReq", false);//if we use true, we get internal server error. And any way we have what we need
            Insert("extraParams", "");
            return this;
        }

        /// <summary>
        /// Sets the fast link token.
        /// </summary>
        /// <param name="fastlinkToken">The fastlink token.</param>
        /// <returns></returns>
        public YFastLinkRequest SetFastLinkToken(string fastlinkToken) {
            Insert("token", fastlinkToken);
            return this;
        }


        /// <summary>
        /// Sets the fast link site.
        /// In this case user can link only this specific site
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <returns></returns>
        public YFastLinkRequest SetOptionalFastLinkSite(int siteId) {
            Insert("extraParams", string.Format("flow=add&siteId={0}", siteId));
            return this;
        }

        /// <summary>
        /// Gets the get form HTML.
        /// </summary>
        /// <value>
        /// The get form HTML.
        /// </value>
        public string GetFormHtml
        {
            get
            {
                var doc = this.Build()
                    .ToDictionary(o => o.Key, o => o.Value);
                return string.Format(formHtmlContent, "https://node.developer.yodlee.com/authenticate/restserver/",
                    doc[Rsession], doc["app"], doc["token"], doc["extraParams"]);
            }
        }
    }
}
