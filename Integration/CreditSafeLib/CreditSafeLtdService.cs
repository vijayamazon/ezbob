namespace Ezbob.CreditSafeLib
{
	using System.Xml.Linq;
	using Ezbob.CreditSafeLib.CreditSafeServiceReference;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using Ezbob.Backend.ModelsWithDB;
    using EzServiceAccessor;
    using StructureMap;

    public class CreditSafeLtdService
    {
        private static readonly SafeILog ms_oLog = new SafeILog(typeof(CreditSafeLtdService));

        public MP_ServiceLog ServiceLogCreditSafeLtdData(string regNumber, int customerId)
        {
            ms_oLog.Debug("Downloading data from CreditSafe for company {0} and customer {1}...", regNumber, customerId);

            string requestXml = GetResource("Ezbob.CreditSafeLib.Templates.CreditSafeLtdRequestTemplate.xml", regNumber);
            
            CreditsafeServicesSoapClient client = new CreditsafeServicesSoapClient("CreditsafeServicesSoap");
            string newResponse = client.GetData(requestXml);

            var pkg = new WriteToLogPackage(requestXml, newResponse, ExperianServiceType.CreditSafeLtd, customerId, companyRefNum: regNumber);

            ObjectFactory.GetInstance<IEzServiceAccessor>().ServiceLogWriter(pkg);

            ms_oLog.Debug("Downloading data from CreditSafe for company {0} and customer {1} complete.", regNumber, customerId);

            return pkg.Out.ServiceLog;
        }
        private string GenerateRequestXML(string refNum)
        {
            //build the request XML based on the values entered into the form
            string strRequest = "";
            XDocument xmldoc = XDocument.Load("Templates\\CreditSafeLtdRequestTemplate.xml");

            string template = xmldoc.ToString();
            return string.Format(template, refNum);
        }
        private string GetResource(string resName, params object[] p) {
            //string template = @"<xmlrequest><header><username>ORAN01</username><password>Jd4xDKpy</password><operation>GetCompanyInformation</operation><language>EN</language><chargereference></chargereference></header><body><package>standard</package><country>UK</country><companynumber>{0}</companynumber></body></xmlrequest>";
            using (Stream s = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(resName))
            {
                if (s == null)
                    return null;

                var data = new byte[s.Length];

                s.Read(data, 0, (int)s.Length);

                string template =  Encoding.UTF8.GetString(data);
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(template);
                string xmlStr = template.ToString();
                return string.Format(xmlStr, p);
            } // using


        } // GetResource
    }
}
