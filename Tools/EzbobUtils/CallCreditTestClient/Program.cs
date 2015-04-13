namespace Ezbob.Integration.CallCreditLib {
	using System;
	using Callcredit.CRBSB;

	public class Program {

		// to ConfigurationVariables db table
		private static string password = "7UM9AXH2";
		private static string userName = "Ezbob SR API CTEST";
		private static string companyName = "Ezbob SR CTEST";

		private static void Main(string[] args) {

			try {
				CallcreditBsbAndCreditReport apiProxy = InitializeApiProxy();

				UserInfo user = InitializeUser();
				CT_SearchDefinition apiSD = InitializeApiRequest(user);
				CT_SearchResult apiresult = new CT_SearchResult();

				apiresult = apiProxy.Search07a(apiSD);


				var builder = new CallCreditModelBuilder();

				builder.Build(apiresult);

				Console.WriteLine("====" + apiresult.creditrequest.applicant.Length);
				Console.WriteLine("====" + apiresult.creditrequest.applicant[0].dob);

			} catch (Exception e) {
				Console.WriteLine(e);
				//throw;
			}

			/*apiProxy.Dispose();*/

		} // Main


		private static CallcreditBsbAndCreditReport InitializeApiProxy() {
			/* Create a new proxy object which represents the Callcredit API. */
			CallcreditBsbAndCreditReport apiProxy = new CallcreditBsbAndCreditReport();

			/* We can alter the proxy URL here, if necessary. */
			/* TODO: Select Appropriate URL - either Client Test Site or Live Site */
			//apiProxy.Url = "https://www.callcreditsecure.co.uk/Services/BSB/CRBSB7.asmx";	//Live Site URL
			apiProxy.Url = "https://ct.callcreditsecure.co.uk/Services/BSB/CRBSB7.asmx";	//Client Test Site URL

			/* Create a new callcreditheaders object and attach it to the proxy object. */
			/* TODO: Setup User Credentials (provided by Callcredit Professional Services) */
			callcreditheaders apiCredentials = new callcreditheaders();
			apiCredentials.company = companyName;
			apiCredentials.username = userName;
			apiCredentials.password = password;
			apiProxy.callcreditheadersValue = apiCredentials;

			return apiProxy;
		}

		private static UserInfo InitializeUser() {
			UserInfo user = new UserInfo();

			/*user.dob = new DateTime(1910, 01, 01);
			user.title = "MISS";
			user.forename = "JULIA";
			user.othernames = "";
			user.surname = "AUDI";
			user.buildingno = "1";
			user.street = "TOP GEAR LANE";
			user.postcode = "X9 9LF";*/

			user.dob = new DateTime(1960, 11, 05);
			user.title = "MR";
			user.forename = "OSCAR";
			user.othernames = "TEST-PERSON";
			user.surname = "MANX";
			user.buildingno = "606";
			user.street = "ALLEY CAT LANE";
			user.postcode = "X9 9AA";

			return user;
		}

		private static CT_SearchDefinition InitializeApiRequest(UserInfo user) {
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

			/* Create a new request applicant object and attach it to the credit request object. */
			CT_searchapplicant apiApplicant = new CT_searchapplicant();
			apiApplicant.dob = user.dob;
			apiApplicant.dobSpecified = true;

			srequest.applicant = new CT_searchapplicant[] { apiApplicant };

			/* Create a new name object and attach it to the request applicant object. */
			CT_inputname apiName = new CT_inputname();
			apiName.title = user.title;
			apiName.forename = user.forename;
			apiName.othernames = user.othernames;
			apiName.surname = user.surname;

			apiApplicant.name = new CT_inputname[] { apiName };

			/* Create a new input current address object */
			CT_inputaddress apiInputCurrentAddress = new CT_inputaddress();
			apiInputCurrentAddress.buildingno = user.buildingno;
			apiInputCurrentAddress.street1 = user.street;
			apiInputCurrentAddress.postcode = user.postcode;

			apiApplicant.address = new CT_inputaddress[] { apiInputCurrentAddress };

			return searchDef;
		}

	}
}
