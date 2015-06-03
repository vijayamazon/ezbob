namespace Ezbob.Backend.Strategies.CreditSafe
{
    using System;
    using System.IO;
    using System.Text;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;
    using ConfigManager;
    using Ezbob.Backend.ModelsWithDB;
    using Ezbob.Backend.Strategies.Misc;
    using Ezbob.CreditSafeLib;
    using Ezbob.CreditSafeLib.CreditSafeServiceReference;
    using Ezbob.Database;
    using Ezbob.Logger;
    using EZBob.DatabaseLib.Model.Database;

    public class ServiceLogCreditSafeNonLtd :AStrategy
    {
        public ServiceLogCreditSafeNonLtd(int customerId) {
            this.o_customerId = customerId;
        }
        public override string Name {
            get { return "ServiceLogCreditSafeNonLtd"; }
        }

        public override void Execute() {
            ServiceLogCreditSafeNonLtdData(o_customerId);
        }
        public void ServiceLogCreditSafeNonLtdData(int customerId)
        {
            SafeReader addressSR = this.m_oDB.GetFirst(
                "GetCompanyAddress",
                CommandSpecies.StoredProcedure,
                new QueryParameter("CustomerId", customerId)
                );
            SafeReader companySR = this.m_oDB.GetFirst(
                "GetCompanyName",
                CommandSpecies.StoredProcedure,
                new QueryParameter("CustomerId", customerId)
            );

            string companyName = companySR["CompanyName"];
            if (string.IsNullOrEmpty(companyName))
            {
                Log.Info("CreditSafeNonLtd not retrieving data - customer has no company name.");
                return;
            }
            string companyId = companySR["CompanyId"];
            string address = addressSR["Line1"];
            string postcode = addressSR["Postcode"];
            string uname = CurrentValues.Instance.CreditSafeUserName;
            string pass = CurrentValues.Instance.CreditSafePassword;

            Log.Debug("Targeting data from CreditSafe for Company: {0} With CompanyId:{1} and CustomerId: {2}", companyName, companyId, customerId);

            string targetingRequest = GetResource("Ezbob.Backend.Strategies.CreditSafe.Tamplets.CreditSafeNonLtdSearchTemplate.xml", uname, pass, companyName, address, postcode);

            CreditsafeServicesSoapClient client = new CreditsafeServicesSoapClient("CreditsafeServicesSoap");
            string targetingResponse = client.GetData(targetingRequest);

            var targetingPkg = new WriteToLogPackage(targetingRequest, targetingResponse, ExperianServiceType.CreditSafeNonLtdTargeting, customerId);
            new ServiceLogWriter(targetingPkg).Execute();

            XmlSerializer searchSerializer = new XmlSerializer(typeof(CreditSafeNonLtdSearchResponse), new XmlRootAttribute("xmlresponse"));
            CreditSafeNonLtdSearchResponse targetingResult = (CreditSafeNonLtdSearchResponse)searchSerializer.Deserialize(new StringReader(targetingResponse));

            try
            {
                xmlresponsesearchBody body = (xmlresponsesearchBody)targetingResult.Items[1];
                if (body.results != null && body.results.Length == 1)
                {
                    string companyNum = body.results[0].number;
                    Log.Debug("Downloading data from CreditSafeNonLtd for company {0} and customer {1}...", companyNum, customerId);
                    string requestXml = GetResource("Ezbob.Backend.Strategies.CreditSafe.Tamplets.CreditSafeNonLtdRequestTemplate.xml", uname, pass, companyNum);
                    string newResponse = client.GetData(requestXml);
                    var pkg = new WriteToLogPackage(requestXml, newResponse, ExperianServiceType.CreditSafeNonLtd, customerId, companyRefNum: companyNum);
					new ServiceLogWriter(pkg).Execute();
                    Log.Debug("Downloading data from CreditSafeNonLtd for company {0} and customer {1} complete.", companyNum, customerId);
                }//if
                else
                {
                    Log.Alert("CreditSafeNonLtd Targeting failed for customer {0}", customerId);
                }//else
            }//try
            catch (Exception ex)
            {
                Log.Error(ex, "failed to retrieve CreditSafe data for customerId:{0}", customerId);
            }//catch
        }//ServiceLogCreditSafeNonLtdData

        /// <summary>
        /// function extructs embedded text resources then formats the extructed with the parameters
        /// </summary>
        /// <param name="resName">Resource name</param>
        /// <param name="p">parameters to be formated to the resource</param>
        /// <returns>the extructed resource with formated parameters in string form</returns>
        private string GetResource(string resName, params object[] p)
        {
            using (Stream s = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(resName))
            {
                if (s == null)
                    return null;

                var data = new byte[s.Length];

                s.Read(data, 0, (int)s.Length);

                string template = Encoding.UTF8.GetString(data);
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(template);
                string xmlStr = template.ToString();
                return string.Format(xmlStr, p);
            } // using
        } // GetResource

        private static readonly SafeILog Log = new SafeILog(typeof(ServiceLogCreditSafeNonLtd));
        private readonly AConnection m_oDB = new SqlConnection();
        private int o_customerId;
    }
}
