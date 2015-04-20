using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Serialization;
using Ezbob.Backend.ModelsWithDB.CreditSafe;
using Ezbob.CreditSafeLib.CreditSafeServiceReference;

namespace Ezbob.CreditSafeLib
{
    public class CreditSafeLtdGetData
    {
        public void LtdGetData(string companyNunmber)
        {
            CreditsafeServicesSoapClient client = new CreditsafeServicesSoapClient("CreditsafeServicesSoap");


            var builder = new CreditsafeRequestBuilder();
            var model = new CreditsafeRequestModel();
            model.UserName = "ORAN01";
            model.Password = "Jd4xDKpy";
            model.Operation = "GetCompanyInformation";
            //model.Operation = "GetNonLtdCompanyInformation";
            model.ChargeReference = "";
            model.Package = "standard";
            //model.Package = "nonltdstandard";
            model.Country = "UK";
            //model.companynumber = "3697636";
            //model.companynumber = companyNunmber;
            model.companynumber = "X9999999";
            string request = builder.GenerateRequestXML(model);
            //string request = "<xmlrequest><header><username>ORAN01</username><password>Jd4xDKpy</password><operation>GetCompanyInformation</operation><language>EN</language><chargereference></chargereference></header><body><package>standard</package><country>UK</country><companynumber>X9999999</companynumber></body></xmlrequest>";
            string response = client.GetData(request);

            XmlSerializer serializer = new XmlSerializer(typeof(CreditSafeLtdResponse),new XmlRootAttribute("xmlresponse"));
            CreditSafeLtdResponse ei = (CreditSafeLtdResponse)serializer.Deserialize(new StringReader(response));
            CreditSafeLtdModelBuilder ModelBuilder = new CreditSafeLtdModelBuilder();
            CreditSafeBaseData BaseData = ModelBuilder.Build(ei);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(response);
            doc.Save("response.xml");
        }
    }
}
