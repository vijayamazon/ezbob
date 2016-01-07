namespace EzBob3dParties.HMRC {
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Common.Logging;
    using EzBobCommon;
    using EzBobCommon.Utils.Encryption;
    using EzBobCommon.Web;
    using HtmlAgilityPack;

    public class HmrcService : IHmrcService {
        private static readonly string Href = "href";

        private static readonly string baseAddress = "https://online.hmrc.gov.uk";
        private static readonly int maxPasswordLength = 12;

        [Injected]
        public ILog Log { get; set; }

        [Injected]
        public LoginDetailsScraper LoginDetailsScraper { get; set; }

        [Injected]
        public UserVatIdAndTaxOfficeNumberFetcher VatIdAndTaxOfficeNumberFetcher { get; set; }

        [Injected]
        public VatReturnsInfoFetcher VatReturnsInfoFetcher { get; set; }

        [Injected]
        public RtiTaxYearsFetcher RtiTaxYearsFetcher { get; set; }

        [Injected]
        public IEzBobWebBrowser Browser { get; set; }


        /// <summary>
        /// Validates the credentials.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public async Task<bool> ValidateCredentials(string userName, string password) {
            if (!IsValidUserNamePassword(userName, password)) {
                return false;
            }

            return await Login(userName, password);
        }

        /// <summary>
        /// Gets the vat returns.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public async Task<HmrcVatReturnsInfo> GetVatReturns(string userName, string password) {
            if (!IsValidUserNamePassword(userName, password)) {
                return null;
            }

            bool isLoggedIn = await Login(userName, password);
            if (!isLoggedIn) {
                return null;
            }

            //try to obtain user's VAT id
            var taxOfficeNumberAndVatId = await VatIdAndTaxOfficeNumberFetcher.GetUserVatIdAndTaxOfficeNumber(baseAddress, Browser);
            if (string.IsNullOrEmpty(taxOfficeNumberAndVatId.VatId)) {
                return null;
            }

            RtiTaxYearInfo yearInfo = null;

            if (string.IsNullOrEmpty(taxOfficeNumberAndVatId.TaxOfficeNumber)) {
                Log.Debug("Not fetching RTI Tax Years: Tax Office number is empty.");
            } else {
                yearInfo = await RtiTaxYearsFetcher.GetRtiTaxYears(taxOfficeNumberAndVatId.TaxOfficeNumber, baseAddress, Browser);
            }

            IEnumerable<VatReturnInfo> vatReturnInfos = await VatReturnsInfoFetcher.GetVatReturns(taxOfficeNumberAndVatId.VatId, baseAddress, Browser);
            return new HmrcVatReturnsInfo {
                TaxOfficeNumber = taxOfficeNumberAndVatId.TaxOfficeNumber,
                VatReturnInfos = vatReturnInfos,
                RtiTaxYearInfo = yearInfo
            };
        }

        /// <summary>
        /// Logins the specified user name.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private async Task<bool> Login(string userName, string password) {

            if (password.Length > maxPasswordLength) {
                Log.Warn(string.Format("Supplied password ({0}) is too long, truncating to {1} characters.", EncryptionUtils.Encrypt(password), maxPasswordLength));
                password = password.Substring(0, maxPasswordLength);
            }

            try {
                //Scrap login details
                ScrapedLoginRequestDetails scrapedLoginDetails = await LoginDetailsScraper.ScrapLoginDetails(baseAddress, Browser);
                if (!IsLoginRequestDetailsScrapedAsExpected(scrapedLoginDetails)) {
                    return false;
                }

                Log.InfoFormat("Login URL: {0}", scrapedLoginDetails.LoginPageUrl);

                //try to login
                string loginResponse = await HttpPostLogin(scrapedLoginDetails, userName, password);
                if (string.IsNullOrEmpty(loginResponse) || !IsLoginSucceeded(loginResponse, userName)) {
                    return false;
                }
            } catch (Exception ex) {
                Log.Error(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether scrapedLogin succeeded or not.
        /// </summary>
        /// <param name="loginResponse">The scrapedLogin response.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        private bool IsLoginSucceeded(string loginResponse, string userName) {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(loginResponse);

            HtmlNode title = doc.DocumentNode.SelectSingleNode("/html/head/title");

            if (title == null) {
                Log.Error("No title in scrapedLogin response:");
                Log.Debug(loginResponse);
                return false;
            }

            bool IsLoggedIn;

            switch (title.InnerText.Trim()) {
            case "HMRC: Security message":
                IsLoggedIn = true;
                Log.InfoFormat("User {0} logged in successfully.", userName);
                break;

            case "HMRC: Error":
                IsLoggedIn = false;
                Log.Error("HMRC system is unavailable, please try again later.");
                break;

            case "HMRC: Login":
                IsLoggedIn = false;
                Log.Error("Not logged in: invalid user name or password.");
                break;

            default:
                IsLoggedIn = false;
                Log.ErrorFormat("Not logged in because of unexpected page title in scrapedLogin response: {0}", title.InnerText);
                break;
            }

            Log.Debug(loginResponse);
            return IsLoggedIn;
        }

        /// <summary>
        /// Posts the scraped Login.
        /// </summary>
        /// <param name="scrapedLoginDetails">The scraped Login details.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private Task<string> HttpPostLogin(ScrapedLoginRequestDetails scrapedLoginDetails, string userName, string password) {
            Log.InfoFormat("Logging in as {0}:{1}...", userName, EncryptionUtils.Encrypt(password));

            var dictionary = new Dictionary<string, string> {
                {
                    scrapedLoginDetails.UserName, userName
                }, {
                    scrapedLoginDetails.Password, password
                }
            };

            var encodedContent = new FormUrlEncodedContent(dictionary);

            return Browser.PostAsyncAndGetStringResponse(baseAddress + scrapedLoginDetails.LoginPageUrl, encodedContent);
        }

        /// <summary>
        /// Determines whether we got expected scraped login request details.
        /// </summary>
        /// <param name="scrapedLogin">The scraped login request.</param>
        /// <returns></returns>
        private bool IsLoginRequestDetailsScrapedAsExpected(ScrapedLoginRequestDetails scrapedLogin) {

            if (string.IsNullOrEmpty(scrapedLogin.HttpMethod) || string.IsNullOrEmpty(scrapedLogin.LoginPageUrl) || string.IsNullOrEmpty(scrapedLogin.Password) || string.IsNullOrEmpty(scrapedLogin.UserName)) {
                return false;
            }

            if (!scrapedLogin.HttpMethod.ToUpperInvariant()
                   .Equals("POST"))
            {
                Log.Error("Unsupported scrapedLogin method: " + scrapedLogin.HttpMethod);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates user name and password.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private bool IsValidUserNamePassword(string userName, string password) {
            if (string.IsNullOrEmpty(userName))
            {
                Log.Error("got empty user name");
                return false;
            }

            if (string.IsNullOrEmpty(password))
            {
                Log.Error("got empty password");
                return false;
            }

            return true;
        }
    }
}
