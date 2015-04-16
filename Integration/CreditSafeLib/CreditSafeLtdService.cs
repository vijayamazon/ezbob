using System.Xml.Linq;
using Ezbob.CreditSafeLib.CreditSafeServiceReference;
using Ezbob.Logger;
using EZBob.DatabaseLib.Model.Database;

namespace Ezbob.CreditSafeLib
{
    using Ezbob.Backend.ModelsWithDB;
    using EzServiceAccessor;
    using StructureMap;

    public class CreditSafeLtdService
    {
        private static readonly SafeILog ms_oLog = new SafeILog(typeof(CreditSafeLtdService));

        public MP_ServiceLog ServiceLogCreditSafeLtdData(string regNumber, int customerId)
        {
            ms_oLog.Debug("Downloading data from CreditSafe for company {0} and customer {1}...", regNumber, customerId);

            string requestXml = GenerateRequestXML(regNumber);
            
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
    }
}
