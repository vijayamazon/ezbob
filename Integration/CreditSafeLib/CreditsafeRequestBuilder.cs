using System.Xml.Linq;

namespace Ezbob.CreditSafeLib
{
    public class CreditsafeRequestBuilder
    {

        public string GenerateRequestXML(CreditsafeRequestModel model)
        {
            //build the request XML based on the values entered into the form
            string strRequest = "";
            XDocument xmldoc = XDocument.Load("CreditSafeRequestTemplate.xml");

            string template = xmldoc.ToString();
            return string.Format(template, model.UserName, model.Password, model.Operation, model.ChargeReference, model.Package, model.Country, model.companynumber);
        }
    }
}