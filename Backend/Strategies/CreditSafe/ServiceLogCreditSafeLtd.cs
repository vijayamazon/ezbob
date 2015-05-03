

namespace Ezbob.Backend.Strategies.CreditSafe
{
    using System.Text;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using ConfigManager;
    using Ezbob.Backend.ModelsWithDB;
    using Ezbob.CreditSafeLib;
    using Ezbob.CreditSafeLib.CreditSafeServiceReference;
    using Ezbob.Database;
    using Ezbob.Logger;
    using EzServiceAccessor;
    using StructureMap;
    using EZBob.DatabaseLib.Model.Database;

    public class ServiceLogCreditSafeLtd :AStrategy
    {
        public ServiceLogCreditSafeLtd(string regNumber, int customerId) {
            this.o_regNumber = regNumber;
            this.o_customerId = customerId;
        }
        public override string Name {
            get { return "ServiceLogCreditSafeLtd"; }
        }

        public override void Execute() {
            ServiceLogCreditSafeLtdData(o_regNumber, o_customerId);
        }

        public void ServiceLogCreditSafeLtdData(string regNumber, int customerId)
        {
            Log.Debug("Downloading data from CreditSafe for company {0} and customer {1}...", regNumber, customerId);

            string uname = CurrentValues.Instance.CreditSafeUserName;
            string pass = CurrentValues.Instance.CreditSafePassword;
            string requestXml = GetResource("Ezbob.Backend.Strategies.CreditSafe.Tamplets.CreditSafeLtdRequestTemplate.xml", uname, pass, regNumber);

            CreditsafeServicesSoapClient client = new CreditsafeServicesSoapClient("CreditsafeServicesSoap");
            string newResponse = client.GetData(requestXml);

            var pkg = new WriteToLogPackage(requestXml, newResponse, ExperianServiceType.CreditSafeLtd, customerId, companyRefNum: regNumber);

            ObjectFactory.GetInstance<IEzServiceAccessor>().ServiceLogWriter(pkg);

            Log.Debug("Downloading data from CreditSafe for company {0} and customer {1} complete.", regNumber, customerId);
        }//ServiceLogCreditSafeLtdData

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

        private string o_regNumber;
        private int o_customerId;
        private static readonly SafeILog Log = new SafeILog(typeof(ServiceLogCreditSafeLtd));
        private readonly AConnection m_oDB = new SqlConnection();
    }
}
