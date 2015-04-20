using System;
using Callcredit.CRBSB;

namespace CallCreditLib {
	public class CallCreditGetData {
		private readonly CallcreditBsbAndCreditReport apiProxy;
		private readonly CT_SearchDefinition apiSD;

		public CallCreditGetData(CT_searchapplicant searchApplicant) {
			try {
				apiProxy = InitializeApiProxy();

				apiSD = InitializeApiRequest(searchApplicant);
			} catch (Exception e) {
				Console.WriteLine(e);
				//throw;
			}
		}

		public CT_SearchResult GetSearch07a() {

			CT_SearchResult apiresult = new CT_SearchResult();
			apiresult = apiProxy.Search07a(apiSD);
			return apiresult;
			//XmlSerializer serializer = new XmlSerializer(typeof(CT_SearchResult));
			//TextWriter writer = new StreamWriter(@"C:\temp1\Xml.xml");
			//serializer.Serialize(writer, apiresult);
			//CallCreditModelBuilder modelbuilder = new CallCreditModelBuilder();
			//CallCredit BaseData  = modelbuilder.Build(apiresult);
			//BaseData.Apiresult = apiresult;
			//return BaseData;
		}


		private static CallcreditBsbAndCreditReport InitializeApiProxy() {
			/* Create a new proxy object which represents the Callcredit API. */
			CallcreditBsbAndCreditReport apiProxy = new CallcreditBsbAndCreditReport();

			/* We can alter the proxy URL here, if necessary. */
			/* Select Appropriate URL - either Client Test Site or Live Site */
			// "https://www.callcreditsecure.co.uk/Services/BSB/CRBSB7.asmx";	//Live Site URL
			// "https://ct.callcreditsecure.co.uk/Services/BSB/CRBSB7.asmx";	//Client Test Site URL
			apiProxy.Url = ConfigManager.CurrentValues.Instance.CallCreditUrl;

			/* Create a new callcreditheaders object and attach it to the proxy object. */

			callcreditheaders apiCredentials = new callcreditheaders();
			apiCredentials.company = ConfigManager.CurrentValues.Instance.CallCreditUserCompany;
			apiCredentials.username = ConfigManager.CurrentValues.Instance.CallCreditUserName;
			apiCredentials.password = ConfigManager.CurrentValues.Instance.CallCreditPassword;
			apiProxy.callcreditheadersValue = apiCredentials;

			return apiProxy;
		}

		private static CT_SearchDefinition InitializeApiRequest(CT_searchapplicant searchApplicant) {
			CT_SearchDefinition searchDef = new CT_SearchDefinition();

			CT_searchrequest srequest = new CT_searchrequest();

			srequest.purpose = "DS";
			srequest.score = 1;
			srequest.scoreSpecified = true;
			srequest.transient = 0;
			srequest.transientSpecified = true;
			srequest.schemaversion = "7.2";
			srequest.datasets = 511;
			//srequest.credittype = this.cboCreditType.SelectedValue.ToString();
			searchDef.creditrequest = srequest;
			srequest.applicant = new CT_searchapplicant[] { searchApplicant };

			return searchDef;
		}
	}
}
