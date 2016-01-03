namespace EzBob3dParties.HMRC
{
    using System.Linq;
    using System.Threading.Tasks;
    using EzBobCommon;
    using EzBobCommon.Web;
    using HtmlAgilityPack;
    using log4net;

    public class UserVatIdAndTaxOfficeNumberFetcher {
        private static readonly string Href = "href";

        [Injected]
        public ILog Log { get; set; }

        /// <summary>
        /// Gets the user vat identifier and tax office number.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public async Task<TaxOfficeNumberAndVatId> GetUserVatIdAndTaxOfficeNumber(string url, IEzBobWebBrowser browser)
        {
            string html = await browser.DownloadPageAsyncAsString(url + "/home/services");

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            InfoAccumulator info = new InfoAccumulator();
            string vatId = GetUserVatIdOldFashionedWay(document, info);
            if (string.IsNullOrEmpty(vatId))
            {
                vatId = GetUserVatIdNewFashionedWay(document, info);
                if (string.IsNullOrEmpty(vatId)) {
                    LogErrors(info);
                    return new TaxOfficeNumberAndVatId(null, null);
                }
            }

            info = new InfoAccumulator();
            string taxOfficeNumber = ExtractTaxOfficeNumber(document, info);
            if (string.IsNullOrEmpty(taxOfficeNumber)) {
                LogErrors(info);
            }

            return new TaxOfficeNumberAndVatId(taxOfficeNumber, vatId);
        }

        private void LogErrors(InfoAccumulator info) {
            Log.Error(info.GetErrors()
                       .Aggregate("", (o1, o2) => o1 + o2 + ";"));
        }

        private string ExtractTaxOfficeNumber(HtmlDocument doc, InfoAccumulator info) {
            const string sBaseXPath = "//dl[contains(@class, 'known-facts')]";

            HtmlNodeCollection dataLists = doc.DocumentNode.SelectNodes(sBaseXPath);

            if (dataLists == null)
            {
                Log.Warn("No suitable location for Tax Office Number found.");
                return null;
            } // if

            foreach (HtmlNode dl in dataLists)
            {
                HtmlNode dt = dl.SelectSingleNode("./dt");

                if (dt == null)
                    continue;

                if (dt.InnerText != "Tax Office Number:")
                    continue;

                HtmlNode dd = dl.SelectSingleNode("./dd");

                if (dd == null) {
                    info.AddError("Tax Office Number not found.");
                    return null;
                }
                
                var taxOfficeNumber = dd.InnerText.Trim().Replace(" ", "");

                if (taxOfficeNumber == string.Empty) {
                    info.AddError("Tax Office Number not specified.");
                    return null;
                }

                Log.InfoFormat("Tax office number is {0}.", taxOfficeNumber);
                return taxOfficeNumber;
            } // for each data list

            info.AddWarning("Tax Office Number location not found.");
            Log.Warn("Tax Office Number location not found.");
            return null;
        } // ExtractTaxOfficeNumber

        /// <summary>
        /// Gets the user vat identifier new fashioned way.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        private string GetUserVatIdNewFashionedWay(HtmlDocument doc, InfoAccumulator info)
        {

            HtmlNode section = doc.DocumentNode.SelectSingleNode("//section[@id=\"section-vat\"]");

            if (section == null)
            {
                info.AddError("VAT section not found (new fashion way).");
                return null;
            } // if

            if (!section.HasChildNodes)
            {
                info.AddError("VAT section is empty (new fashion way).");
                return null;
            } // if

            HtmlNode para = section.Element("p");

            if (para == null)
            {
                info.AddError("VAT id location not found (new fashion way).");
                return null;
            } // if

            string innerText = (para.InnerText ?? string.Empty);

            if (!innerText.StartsWith("VAT registration number"))
            {
                info.AddError("Failed to parse VAT id location (new fashion way).");
                return null;
            } // if

            string id = innerText.Substring(innerText.IndexOf(':') + 1).Trim();

            if (string.IsNullOrEmpty(id))
            {
                info.AddError("Could not extract user VAT id in a new fashion way.");
                return null;
            } // if

            return id;
        } // GetUserVatIDNewFashionWay

        /// <summary>
        /// Gets the user vat identifier in old fashioned way.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns></returns>
        private string GetUserVatIdOldFashionedWay(HtmlDocument doc, InfoAccumulator info)
        {

            HtmlNode link =
                doc.DocumentNode.SelectSingleNode("//a[@id=\"LinkAccessVAT\"]")
                ??
                doc.DocumentNode.SelectSingleNode("//a[@id=\"LinkAccessVATMakeVATReturn\"]");

            if (link == null)
            {
                info.AddError("Access VAT services link not found (old fashioned way).");
                return null;
            }

            if (!link.Attributes.Contains(Href))
            {
                info.AddError("Access VAT services link has no HREF attribute (old fashioned way).");
                return null;
            }

            string href = link.Attributes[Href].Value;

            if (!href.StartsWith("/vat/trader/"))
            {
                info.AddError("Failed to parse Access VAT services link (old fashioned way).");
                return null;
            }

            string id = href.Substring(href.LastIndexOf('/') + 1).Trim();

            if (string.IsNullOrEmpty(id))
            {
                info.AddError("Could not extract user VAT id in an old fashioned way.");
                return null;
            } // if

            return id;
        } // GetUserVatIDOldFashionWay
    }
}
