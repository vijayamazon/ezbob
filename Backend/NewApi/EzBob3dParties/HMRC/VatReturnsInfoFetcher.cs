namespace EzBob3dParties.HMRC {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Logging;
    using EzBobCommon;
    using EzBobCommon.Web;
    using HtmlAgilityPack;

    /// <summary>
    /// Fetches VAT returns info
    /// </summary>
    public class VatReturnsInfoFetcher {
        [Injected]
        public ILog Log { get; set; }

        [Injected]
        public VatReturnInfoParser VatReturnInfoParser { get; set; }

        /// <summary>
        /// Gets the vat returns.
        /// </summary>
        /// <param name="userVatId">The user vat identifier.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="browser">The browser.</param>
        /// <returns></returns>
        public async Task<IEnumerable<VatReturnInfo>> GetVatReturns(string userVatId, string baseUrl, IEzBobWebBrowser browser) {
            InfoAccumulator info = new InfoAccumulator();

            IDictionary<string, string> submittedReturns = await FetchSubmittedReturnsList(userVatId, baseUrl, info, browser);
            if (info.HasErrors || submittedReturns == null || submittedReturns.Count == 0) {
                return null;
            }

            var tasks = this.ObtainVatReturns(submittedReturns, userVatId, baseUrl, browser);
            
            Optional<VatReturnInfo>[] optionals = await Task.WhenAll(tasks);
            
            return optionals.Where(o => o.HasValue)
                .Select(o => o.GetValue());
        }

        /// <summary>
        /// Obtains the vat returns.
        /// </summary>
        /// <param name="submittedReturns">The submitted returns.</param>
        /// <param name="userVatId">The user vat identifier.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="browser">The browser.</param>
        /// <returns></returns>
        private IEnumerable<Task<Optional<VatReturnInfo>>> ObtainVatReturns(IDictionary<string, string> submittedReturns, string userVatId, string baseUrl, IEzBobWebBrowser browser)
        {
            foreach (var submitedReturn in submittedReturns) {
                string htmlUrl = baseUrl + string.Format(
                    "/vat-file/trader/{0}{1}{2}",
                    userVatId,
                    userVatId.EndsWith("/") || submitedReturn.Key.StartsWith("/") ? "" : "/",
                    submitedReturn.Key);

                string pdfUrl = htmlUrl + "?format=pdf";
                string baseFileName = submitedReturn.Value.Replace(' ', '_');

                Log.InfoFormat("VatReturns: requesting {0}.html <- {1}", baseFileName, htmlUrl);

                yield return CreateSingleVatReturnInfo(htmlUrl, pdfUrl, browser);
            }
        }

        /// <summary>
        /// Creates the single vat return information.
        /// </summary>
        /// <param name="htmlUrl">The HTML URL.</param>
        /// <param name="pdfUrl">The PDF URL.</param>
        /// <param name="browser">The browser.</param>
        /// <returns></returns>
        private async Task<Optional<VatReturnInfo>> CreateSingleVatReturnInfo(string htmlUrl, string pdfUrl, IEzBobWebBrowser browser) {

            string html = await browser.DownloadPageAsyncAsString(htmlUrl);
            Optional<VatReturnInfo> optional = this.ParseVatReturnHtml(html);
            if (optional.HasValue) {
                byte[] pdfFile = await browser.DownloadPageAsyncAsByteArray(pdfUrl);
                optional.GetValue()
                    .PdfFile = pdfFile;
            }
            return optional;    
        }

        /// <summary>
        /// Parses the vat return HTML.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <returns></returns>
        private Optional<VatReturnInfo> ParseVatReturnHtml(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            Optional<VatReturnInfo> vatReturnInfo = VatReturnInfoParser.ParseVatReturnInfo(doc);
            return vatReturnInfo;
        }

        /// <summary>
        /// Fetches the submitted returns list.
        /// </summary>
        /// <param name="vatId">The vat identifier.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="info">The information.</param>
        /// <param name="browser">The browser.</param>
        /// <returns></returns>
        private async Task<IDictionary<string, string>> FetchSubmittedReturnsList(string vatId, string baseUrl, InfoAccumulator info, IEzBobWebBrowser browser) {
            Log.Info("Loading list of submitted VAT returns...");

            string html = await browser.DownloadPageAsyncAsString(baseUrl + "/vat-file/trader/" + vatId + "/periods");

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            bool hasNoPrevious = doc.DocumentNode.InnerText.IndexOf(
                "There are no returns previously submitted available to view.",
                StringComparison.InvariantCulture
                ) >= 0;

            if (hasNoPrevious) {
                Log.Info("There are no returns previously submitted available to view.");
                return null;
            } // if

            HtmlNode listTable = doc.DocumentNode.SelectSingleNode("//*[@id=\"VAT0011\"]/div[2]/table/tbody");

            if (listTable == null) {
                info.AddError("Failed to find list of returns.");
                return null;
            }

            if (listTable.ChildNodes == null) {
                Log.Info("Loading list of submitted VAT returns complete, no files found.");
                return null;
            } // if

            var res = new Dictionary<string, string>();

            foreach (HtmlNode tr in listTable.ChildNodes) {
                if (tr.Name.ToUpper() != "TR")
                    continue;

                if (tr.ChildNodes == null)
                    continue;

                HtmlNode td = tr.SelectSingleNode("td");

                if (td == null)
                    continue;

                HtmlNode link = td.SelectSingleNode("a");

                if (link == null)
                    continue;

                if (!link.Attributes.Contains("href"))
                    continue;

                string href = link.Attributes["href"].Value;

                if (string.IsNullOrWhiteSpace(href))
                    continue;

                res[href] = link.InnerText;
            } // for each row

            Log.InfoFormat(
                "Loading list of submitted VAT returns complete, {0} file{1} found.",
                res.Count,
                res.Count == 1 ? "" : "s"
                );

            return res;
        } // Load
    }
}
