using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.HMRC {
    using System.IO;
    using System.Text.RegularExpressions;
    using EzBobCommon;
    using HtmlAgilityPack;
    using log4net;

    public class RtiTaxYearParser {
        private static readonly string OneMonthPattern = @"(\d+)[^ ]+ (Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)";

        [Injected]
        public ILog Log { get; set; }

        [Injected]
        public GBPParser GbpParser { get; set; }

        /// <summary>
        /// Parses the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.InvalidDataException">
        /// RTI tax years table head is empty.
        /// or
        /// Failed to fetch RTI tax years: no cells in header row
        /// or
        /// RTI tax years table body not found.
        /// or
        /// RTI tax years data not found.
        /// </exception>
        public RtiTaxYearInfo Parse(HtmlDocument document) {
            HtmlNode oTHead = document.DocumentNode.SelectSingleNode("//*[@id=\"top\"]/div[3]/div[2]/div/div[2]/table[1]/thead");

            if (oTHead == null) {
                Log.Info("RTI tax years table head not found.");
                return null;
            } // if

            HtmlNodeCollection oHeadRows = oTHead.SelectNodes("tr");

            if ((oHeadRows == null) || (oHeadRows.Count != 1)) {
                throw new InvalidDataException("RTI tax years table head is empty.");
            }

            HtmlNodeCollection oHeadCells = oHeadRows[0].SelectNodes("th | td");

            string[] aryExpectedColumnHeaders = {
                "Date",
                "Amount paid in period",
                "Amount due in period",
            };

            if ((oHeadCells == null) || (oHeadCells.Count != aryExpectedColumnHeaders.Length)) {
                throw new InvalidDataException("Failed to fetch RTI tax years: no cells in header row");
            }

            for (int i = 0; i < aryExpectedColumnHeaders.Length; i++) {
                if (!oHeadCells[i].InnerText.Trim()
                    .StartsWith(aryExpectedColumnHeaders[i])) {
                    Log.InfoFormat(
                        "Not fetching RTI tax years: unexpected column {0} name: {1} (expected: {2})",
                        i, oHeadCells[i].InnerText, aryExpectedColumnHeaders[i]
                        );
                    return null;
                } // if
            } // for

            HtmlNode oTBody = document.DocumentNode.SelectSingleNode("//*[@id=\"top\"]/div[3]/div[2]/div/div[2]/table[1]/tbody");

            if (oTBody == null) {
                throw new InvalidDataException("RTI tax years table body not found.");
            }

            HtmlNodeCollection oRows = oTBody.SelectNodes("tr");

            if ((oRows == null) || (oRows.Count < 1)) {
                throw new InvalidDataException("RTI tax years data not found.");
            }

            bool isFirst = true;
            int rowNum = -1;

            int firstYear = 0;
            int lastYear = 0;

            var data = new List<RtiTaxMonthSection>();

            foreach (HtmlNode oTR in oRows) {
                rowNum++;

                HtmlNodeCollection cells = oTR.SelectNodes("th | td");

                if ((cells == null) || (cells.Count < 1)) {
                    throw new InvalidDataException(string.Format(
                        "Failed to fetch RTI tax years: no cells in row {0}.",
                        rowNum
                        ));
                } // if

                if (isFirst) {
                    isFirst = false;

                    HtmlNode cell = cells[0];

                    if (!cell.Attributes.Contains("colspan") || (cell.Attributes["colspan"].Value != "3")) {
                        throw new InvalidDataException(string.Format(
                            "Failed to fetch RTI tax years: incorrect format in row {0}",
                            rowNum
                            ));
                    } // if

                    if (cell.InnerText.Trim() == "Previous tax years")
                        break;

                    MatchCollection match = Regex.Matches(cell.InnerText.Trim(), @"^Current tax year (\d\d)(\d\d)-(\d\d)$");

                    if (match.Count != 1) {
                        throw new InvalidDataException(string.Format(
                            "Failed to fetch RTI tax years: incorrect content in row {0}.",
                            rowNum
                            ));
                    } // if

                    GroupCollection grp = match[0].Groups;
                    if (grp.Count != 4) {
                        throw new InvalidDataException(string.Format(
                            "Failed to fetch RTI tax years: unexpected content in row {0}.",
                            rowNum
                            ));
                    } // if

                    firstYear = Convert.ToInt32(grp[1].Value) * 100 + Convert.ToInt32(grp[2].Value);
                    lastYear = Convert.ToInt32(grp[1].Value) * 100 + Convert.ToInt32(grp[3].Value);

                    Log.InfoFormat("Current tax year: {0} - {1}", firstYear, lastYear);

                    continue;
                } // if first row

                string sFirstCell = cells.Count > 0 ? cells[0].InnerText.Trim() : string.Empty;

                if (cells.Count != 3) {
                    if ((cells.Count == 1) && (sFirstCell == "Previous tax years"))
                        break;

                    throw new InvalidDataException(string.Format(
                        "Failed to fetch RTI tax years: unexpected number of cells in row {0}.",
                        rowNum
                        ));
                } // if

                if (sFirstCell == "Total")
                    break;

                try {
                    data.Add(CreateRtiTaxYearSection(sFirstCell, cells[1].InnerText.Trim(), cells[2].InnerText.Trim()));
                } catch (Exception e) {
                    throw new InvalidDataException(
                        string.Format(
                            "Failed to fetch RTI tax years: unexpected format in row {0}.",
                            rowNum
                            ),
                        e
                        );
                } // try
            } // for each row

            RtiTaxYearInfo yearInfo = new RtiTaxYearInfo {
                Months = CreateRttMonthInfos(data, firstYear, lastYear)
            };


            Log.Debug("Fetching RTI Tax Years complete.");

            return yearInfo;
        }

        /// <summary>
        /// Creates the RTT month infos.
        /// </summary>
        /// <param name="sections">The sections.</param>
        /// <param name="firstYear">The first year.</param>
        /// <param name="lastYear">The last year.</param>
        /// <returns></returns>
        private IEnumerable<RtiTaxMonthInfo> CreateRttMonthInfos(IList<RtiTaxMonthSection> sections, int firstYear, int lastYear) {

            int nCurYear = firstYear;

            List<RtiTaxMonthInfo> months = new List<RtiTaxMonthInfo>(15);

            foreach (var section in sections.Reverse()) {
                months.Add(new RtiTaxMonthInfo {
                    DateStart = new DateTime(nCurYear, section.MonthStart, section.DayStart),
                    DateEnd = new DateTime(nCurYear, section.MonthEnd, section.DayEnd),
                    AmountPaid = section.AmountPaid,
                    AmountDue = section.AmountDue
                });

                if (section.MonthStart == 12) {
                    nCurYear = lastYear;
                }
            } // for each

            return months;
        }

        /// <summary>
        /// Creates the rti tax year section.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="amountPaid">The amount paid.</param>
        /// <param name="amountDue">The amount due.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.InvalidDataException">
        /// Unexpected format in period cell.
        /// or
        /// Unexpected format in period cell.
        /// </exception>
        private RtiTaxMonthSection CreateRtiTaxYearSection(string period, string amountPaid, string amountDue) {

            MatchCollection match = Regex.Matches(period, "^" + OneMonthPattern + " - " + OneMonthPattern + "$");

            if (match.Count != 1) {
                throw new InvalidDataException("Unexpected format in period cell.");
            }

            GroupCollection grp = match[0].Groups;

            if (grp.Count != 5) {
                throw new InvalidDataException("Unexpected format in period cell.");
            }

            return new RtiTaxMonthSection {
                DayStart = Convert.ToInt32(grp[1].Value),
                MonthStart = DateTime.ParseExact(grp[2].Value, "MMM", GBPParser.Culture)
                    .Month,

                DayEnd = Convert.ToInt32(grp[3].Value),
                MonthEnd = DateTime.ParseExact(grp[4].Value, "MMM", GBPParser.Culture)
                    .Month,

                AmountPaid = GbpParser.ParseGbpToMoney(amountPaid),
                AmountDue = GbpParser.ParseGbpToMoney(amountDue)
            };
        }
    }
}
