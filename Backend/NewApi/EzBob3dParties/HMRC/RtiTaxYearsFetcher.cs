namespace EzBob3dParties.HMRC {
    using System.IO;
    using System.Threading.Tasks;
    using Common.Logging;
    using EzBobCommon;
    using EzBobCommon.Web;
    using HtmlAgilityPack;

    /// <summary>
    /// RTI (real time information) fetcher
    /// </summary>
    public class RtiTaxYearsFetcher {
        [Injected]
        public ILog Log { get; set; }

        [Injected]
        public RtiTaxYearParser RtiTaxYearParser { get; set; }

        /// <summary>
        /// Gets the rti (real time information) tax years.
        /// </summary>
        /// <param name="taxOfficeNumber">The tax office number.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="browser">The browser.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.InvalidDataException">Failed to fetch PAYE account page.</exception>
        public async Task<RtiTaxYearInfo> GetRtiTaxYears(string taxOfficeNumber, string baseUrl, IEzBobWebBrowser browser) {
            if (!ValidateParameters(taxOfficeNumber, baseUrl)) {
                return null;
            }

            string html = await browser.DownloadPageAsyncAsString(baseUrl + "/paye/org/" + taxOfficeNumber + "/account");
            if (string.IsNullOrEmpty(html)) {
                throw new InvalidDataException("Failed to fetch PAYE account page.");
            }

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            
            return RtiTaxYearParser.Parse(document);
        }

        /// <summary>
        /// Validates the parameters.
        /// </summary>
        /// <param name="taxOfficeNumber">The tax office number.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <returns></returns>
        private bool ValidateParameters(string taxOfficeNumber, string baseUrl) {
            if (string.IsNullOrEmpty(taxOfficeNumber)) {
                Log.Error("got empty tax office number");
                return false;
            }

            if (string.IsNullOrEmpty(baseUrl)) {
                Log.Error("got empty bseUrl");
                return false;
            }

            return true;
        }
    }
}
