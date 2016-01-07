namespace EzBob3dParties.Experian {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Xml.Linq;
    using Common.Logging;
    using EzBob3dParties.Experian.Targeting;
    using EzBobCommon;
    using EzBobModels.ThirdParties.Experian;

    public class Experian : IExperian {
        private static readonly string ExperianESeriesUrl = "http://192.168.100.2:8888/e-series/Controller";

        [Injected]
        public ILog Log { get; set; }

        /// <summary>
        /// Targets the business.
        /// </summary>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="postCode">The post code.</param>
        /// <param name="isLimited">if set to <c>true</c> [is limited].</param>
        /// <param name="regNum">The reg number.</param>
        /// <returns></returns>
        public ResultInfoAccomulator<IEnumerable<Experian3dPartyCompanyInfo>> TargetBusiness(string companyName, string postCode, bool isLimited, string regNum = "") {
            ResultInfoAccomulator<IEnumerable<Experian3dPartyCompanyInfo>> info = new ResultInfoAccomulator<IEnumerable<Experian3dPartyCompanyInfo>>();

            companyName = HttpUtility.HtmlEncode(companyName);
            var xml = new TargetingRequest(companyName, postCode, regNum, isLimited).TransformText();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ExperianESeriesUrl);
            request.Method = "POST";
            request.AllowAutoRedirect = false;
            request.ContentType = "application/xml";
            request.ContentLength = xml.Length;

            var stOut = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
            stOut.Write(xml);
            stOut.Close();

            using (WebResponse resp = request.GetResponse()) {
                Stream stream = resp.GetResponseStream();

                if (stream == null) {
                    LogError("could not read web response.", info);
                    return info;
                }

                using (var sr = new StreamReader(stream)) {
                    string response = sr.ReadToEnd();
                    var companies = this.HandleResponse(response, info);
                    if (info.HasErrors) {
                        return info;
                    }

                    info.Result = companies;
                    return info;
                }
            }
        }

        /// <summary>
        /// Handles the experian response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        private IEnumerable<Experian3dPartyCompanyInfo> HandleResponse(string response, InfoAccumulator info) {
            IList<Experian3dPartyCompanyInfo> companies = new List<Experian3dPartyCompanyInfo>();
            try {
                foreach (var business in XElement.Parse(response)
                    .Element("REQUEST")
                    .Elements("DT11")) {

                    companies.Add(Deserialize<Experian3dPartyCompanyInfo>(business));
                }
            } catch (Exception ex) {
                LogError(ex.ToString(), info);
            }

            return companies;
        }


        private void LogError(string errorMessage, InfoAccumulator info) {
            Log.Error(errorMessage);
            info.AddError(errorMessage);
        }

        /// <summary>
        /// De-serializes the specified element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        private static T Deserialize<T>(XElement element) {

            //TODO: review this implementation
            var t = typeof(T);
            var ret = (T)Activator.CreateInstance(t);
            var cache = t.GetProperties().ToDictionary(pi => pi.Name.ToLowerInvariant());

            foreach (var el in element.Elements())
            {
                if (!cache.ContainsKey(el.Name.LocalName.ToLowerInvariant())) continue;

                var val = el.Value;
                var pi = cache[el.Name.LocalName.ToLowerInvariant()];
                pi.SetValue(ret, Convert.ChangeType(val, pi.PropertyType), null);
            }
            return ret;
        }
    }
}
