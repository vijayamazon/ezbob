namespace EzBob3dParties.Amazon.RatingScraper {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using EzBob3dParties.Amazon.RatingScraper.Feedback;
    using EzBobCommon;
    using EzBobCommon.Web;
    using Fizzler.Systems.HtmlAgilityPack;
    using HtmlAgilityPack;

    /// <summary>
    /// Scraps customer rating from HTML.
    /// </summary>
    internal class AmazonCustomerRatingScraper : IAmazonCustomerRating {
        [Injected]
        public IEzBobHttpClient Browser { get; set; }

        /// <summary>
        /// Gets the rating.
        /// </summary>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <returns></returns>
        public async Task<AmazonCustomerRatingInfo> GetRating(string merchantId) {
            HtmlNode html = await DownloadCustomerPage(merchantId);
            double rating = ExtractRating(html);
            string name = ExtractName(html);
            IEnumerable<FeedbackInfo> feedbacks = ExtractFeedbacks(html);

            return new AmazonCustomerRatingInfo {
                Rating = rating,
                Name = name,
                Feedbacks = feedbacks,
                SubmittedDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Downloads the customer page.
        /// </summary>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <returns></returns>
        private async Task<HtmlNode> DownloadCustomerPage(string merchantId) {
            string url = string.Format(@"http://www.amazon.co.uk/gp/aag/main?ie=UTF8&seller={0}", merchantId);
            string html = await Browser.DownloadPageAsyncAsString(url);
            html = html.Trim();


            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode;
        }

        private IEnumerable<FeedbackInfo> ExtractFeedbacks(HtmlNode node) {
            var extract = node.QuerySelectorAll("table.feedbackTable")
                .ToArray();
            if (extract.Length > 0) {
                return ParseFeedbacks(extract[0].InnerText);
            }

            return Enumerable.Empty<FeedbackInfo>();
        }

        /// <summary>
        /// Parses the feedbacks.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        private IEnumerable<FeedbackInfo> ParseFeedbacks(string text) {
            var rawStrings = text.Split(new[] {
                '\n'
            }, StringSplitOptions.RemoveEmptyEntries)
                .Where(s => s.Trim()
                    .Length > 0 && !s.Contains("?"))
                .Select(s => s.Trim())
                .ToList();


            const int tableSize = 5;

//            var count = rawStrings.Count;
//            Debug.Assert(count == (int)Math.Pow(tableSize, 2));

            for (int i = 1; i < tableSize; i++) {
                for (int j = 1; j < tableSize; j++) {
                    var rawString = rawStrings[(i * tableSize) + j];

                    if (rawString.Contains("%")) {
                        rawString = rawString.Replace("%", "");
                    }

                    int value;
                    int.TryParse(rawString, out value);
                    yield return new FeedbackInfo {
                        Value = value,
                        Type = (FeedbackType)i,
                        Period = (FeedbackPeriod)j
                    };
                }
            }
        }

        /// <summary>
        /// Extracts the name.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private string ExtractName(HtmlNode node) {
            var header = node.QuerySelectorAll("#aag_header > h1")
                .FirstOrDefault();
            if (header == null) {
                return "NoName";
            }
            return header.InnerText;
        }

        /// <summary>
        /// Extracts the rating.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private double ExtractRating(HtmlNode node) {
            double stars = 0.0;
            var extract = node.QuerySelectorAll("div.feedbackMeanRating b")
                .ToArray();
            if (extract.Length == 0) {
                // We don't know how to parse rating from doc
                return stars;
            }

            var textRating = extract[0].InnerText;
            stars = Convert.ToDouble(textRating, CultureInfo.InvariantCulture);
            return stars;
        }
    }
}
