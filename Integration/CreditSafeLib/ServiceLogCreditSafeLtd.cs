using System.Xml.Linq;
using ExperianLib;
using Ezbob.Backend.ModelsWithDB.CreditSafe;
using Ezbob.CreditSafeLib.CreditSafeServiceReference;
using Ezbob.Logger;
using EZBob.DatabaseLib.Model.Database;

namespace Ezbob.CreditSafeLib.Ebussiness
{
    public class ServiceLogCreditSafeLtd
    {
        private static readonly SafeILog ms_oLog = new SafeILog(typeof(ServiceLogCreditSafeLtd));

        public MP_ServiceLog ServiceLogCreditSafeLtdData(string regNumber, int customerId)
        {
            ms_oLog.Debug("Downloading data from CreditSafe for company {0} and customer {1}...", regNumber, customerId);

            string requestXml = GenerateRequestXML(regNumber);
            
            CreditsafeServicesSoapClient client = new CreditsafeServicesSoapClient("CreditsafeServicesSoap");
            string newResponse = client.GetData(requestXml);

            var pkg = new WriteToLogPackage(requestXml, newResponse, ExperianServiceType.CreditSafeLtd, customerId, companyRefNum: regNumber);

            Utils.WriteLog(pkg);

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
    }
}
