namespace EzBob3dParties.HMRC {
    using System.IO;
    using System.Threading.Tasks;
    using Common.Logging;
    using EzBobCommon;
    using EzBobCommon.Web;
    using HtmlAgilityPack;

    public class LoginDetailsScraper {

        [Injected]
        public ILog Log { get; set; }

        /// <summary>
        /// Scraps login details.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="browser">The browser.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.InvalidDataException">Login form not found or too many forms on the page.
        /// or
        /// User name field not found.
        /// or
        /// User name field's NAME attribute not specified.
        /// or
        /// Password field not found.
        /// or
        /// Password field's NAME attribute not specified.</exception>
        public async Task<ScrapedLoginRequestDetails> ScrapLoginDetails(string baseUrl, IEzBobWebBrowser browser) {

            var html = await browser.DownloadPageAsyncAsString(baseUrl);
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            HtmlNodeCollection forms = htmlDocument.DocumentNode.SelectNodes("//form[contains(@action,'login')]");
            if (forms == null || forms.Count != 1) {
                throw new InvalidDataException("Login form not found or too many forms on the page.");
            }

            HtmlNode loginForm = forms[0];

            string loginUrl = loginForm.Attributes["action"].Value;
            string loginMethod = loginForm.Attributes.Contains("method") ? loginForm.Attributes["method"].Value : "GET";

            HtmlNode userName = loginForm.SelectSingleNode("//input[@id=\"FieldUserID\"]");
            if (userName == null)
                throw new InvalidDataException("User name field not found.");

            if (!userName.Attributes.Contains("name"))
                throw new InvalidDataException("User name field's NAME attribute not specified.");

            HtmlNode password = loginForm.SelectSingleNode("//input[@id=\"FieldPassword\"]");
            if (password == null) {
                throw new InvalidDataException("Password field not found.");
            }

            if (!password.Attributes.Contains("name"))
                throw new InvalidDataException("Password field's NAME attribute not specified.");

            var loginDetails = new ScrapedLoginRequestDetails {
                HttpMethod = loginMethod,
                LoginPageUrl = loginUrl,
                UserName = userName.Attributes["name"].Value,
                Password = password.Attributes["name"].Value
            };

            Log.Info(loginDetails.ToString());

            return loginDetails;
        }
    }
}
