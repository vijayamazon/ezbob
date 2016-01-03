using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.HMRC
{
    using System.Globalization;
    using EzBobCommon;
    using EzBobCommon.Currencies;
    using HtmlAgilityPack;
    using log4net;

    public class VatReturnInfoParser
    {
        private static readonly CultureInfo Culture = new CultureInfo("en-GB", false);

        [Injected]
        public ILog Log { get; set; }

        [Injected]
        public DLParser DLParser { get; set; }

        [Injected]
        public GBPParser GbpParser { get; set; }

        public Optional<VatReturnInfo> ParseVatReturnInfo(HtmlDocument document) {
            // Actual XPath expression is //*[@id="VAT0012"]/div[2]/form/div
            // However HtmlAgilityPack parses file wrong and DIVs that should be
            // children of the FORM become siblings of the FORM.

            HtmlNodeCollection divs = document.DocumentNode.SelectNodes("//*[@id=\"VAT0012\"]/div[2]/div");
            if ((divs == null) || (divs.Count != 4))
            {
                Log.WarnFormat("Data sections not found in {0}.", document.DocumentNode.InnerHtml);
                return Optional<VatReturnInfo>.Empty();
            }

            VatReturnInfo vatReturnInfo = new VatReturnInfo();
            this.ParseVatPeriod(divs[0], vatReturnInfo);
            this.ParseBusinessDetails(divs[1], vatReturnInfo);
            this.ParseReturnDetails(divs[2], vatReturnInfo);
            return Optional<VatReturnInfo>.Of(vatReturnInfo);
        }

        private void ParseReturnDetails(HtmlNode oNode, VatReturnInfo vatReturnInfo)
        {
            HtmlNode dl = oNode.SelectSingleNode("dl");

            if (dl == null)
            {
                Log.Warn("Return details not found.");
            } // if

            var parsedData = DLParser.Parse(dl, true);

            if (parsedData == null)
            {
                Log.Info("DL parser failed, not setting return details fields.");
                return;
            } // if

            foreach (KeyValuePair<string, string> pair in parsedData)
            {
                if (pair.Key.Length > 1) {
                    var money = GbpParser.ParseGbpToMoney(pair.Value);
                    string key = pair.Key.Substring(0, pair.Key.Length - 1);
                    vatReturnInfo.ReturnDetails[key] = money;
                    Log.DebugFormat("VatReturnSeeds.ReturnDetails[{0}] = {1}", pair.Key, money.Amount);
                } // if
            } // foreach
        }

        /// <summary>
        /// Parses the business details.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="vatReturnInfo">The vat return information.</param>
        private void ParseBusinessDetails(HtmlNode node, VatReturnInfo vatReturnInfo)
        {
            HtmlNode dl = node.SelectSingleNode("dl");

            if (dl == null)
            {
                Log.Warn("Business details not found.");
                return;
            } // if

            var parsedData = DLParser.Parse(node, false);
            if (parsedData == null)
            {
                Log.Info("DL parser failed, not setting business details fields.");
                return;
            }

            if (parsedData.ContainsKey("VAT Registration Number:"))
            {
                string regNumString = parsedData["VAT Registration Number:"].Replace(" ", "");
                long regNum;
                if (Int64.TryParse(regNumString, out regNum))
                {
                    vatReturnInfo.RegistrationNumber = regNum;
                }
            }

            if (parsedData.ContainsKey("Business name:"))
            {
                vatReturnInfo.BusinessName = parsedData["Business name:"];
            }

            if (parsedData.ContainsKey("Business address:"))
            {
                string[] address = parsedData["Business address:"].Split(new string[] {
                    Environment.NewLine
                }, StringSplitOptions.None);

                vatReturnInfo.BusinessAddress = address;
            }
        }

        /// <summary>
        /// Parses the vat period.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="vatReturnInfo">The vat return information.</param>
        private void ParseVatPeriod(HtmlNode node, VatReturnInfo vatReturnInfo)
        {

            HtmlNode dl = node.SelectSingleNode("dl");
            if (dl == null)
            {
                Log.Warn("VAT period dates not found.");
                return;
            }

            var parsedData = DLParser.Parse(dl, true);
            if (parsedData == null)
            {
                Log.Info("DL parser failed, not setting VAT period fields.");
                return;
            }

            if (parsedData.ContainsKey("Period:"))
            {
                vatReturnInfo.Period = parsedData["Period:"];
            }

            DateTime dt;
            if (parsedData.ContainsKey("Date from:"))
            {
                if (DateTime.TryParseExact(parsedData["Date from:"], "dd MMM yyyy", Culture, DateTimeStyles.None, out dt))
                {
                    vatReturnInfo.FromDate = dt;
                }
            }

            if (parsedData.ContainsKey("Date to:"))
            {
                if (DateTime.TryParseExact(parsedData["Date to:"], "dd MMM yyyy", Culture, DateTimeStyles.None, out dt))
                {
                    vatReturnInfo.ToDate = dt;
                }
            }

            if (parsedData.ContainsKey("Due date:"))
            {
                if (DateTime.TryParseExact(parsedData["Due date:"], "dd MMM yyyy", Culture, DateTimeStyles.None, out dt))
                {
                    vatReturnInfo.DueDate = dt;
                }
            }
        }
    }
}
