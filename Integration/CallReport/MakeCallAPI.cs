using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using Callcredit.CRBSB;

namespace CallReport
{
    class MakeCallAPI
    {
        private static string password = "7UM9AXH2";
        private static string userName = "Ezbob SR API CTEST";
        private static string companyName = "Ezbob SR CTEST";
        public void makeCall()
        {
            CallcreditBsbAndCreditReport apiProxy = InitializeApiProxy();

            UserInfo user = InitializeUser();
            CT_SearchDefinition apiSD = InitializeApiRequest(user);
            CT_SearchResult apiresult = new CT_SearchResult();

            apiresult = apiProxy.Search07a(apiSD);
            apiProxy.Dispose();
        }

        private CallcreditBsbAndCreditReport InitializeApiProxy()
        {
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

        private UserInfo InitializeUser()
        {
            UserInfo user = new UserInfo();

			//user.dob = new DateTime(1910, 01, 01);
			//user.title = "MISS";
			//user.forename = "JULIA";
			//user.othernames = "";
			//user.surname = "AUDI";
			//user.buildingno = "1";
			//user.street = "TOP GEAR LANE";
			//user.postcode = "X9 9LF";

			user.dob = new DateTime(1943, 03, 06);
			user.title = "MR";
			user.forename = "RICHARD";
			user.othernames = "";
			user.surname = "DAEWOO";
			user.buildingno = "10";
			user.street = "TOP GEAR LANE";
			user.postcode = "X9 9LF";

			//user.dob = new DateTime(1978, 01, 27);
			//user.title = "MR";
			//user.forename = "VLAD";
			//user.othernames = "";
			//user.surname = "TREVORSON";
			//user.buildingno = "1";
			//user.street = "SUNNY STREET";
			//user.postcode = "X9 9AY";

            return user;
        }

        private CT_SearchDefinition InitializeApiRequest(UserInfo user)
        {
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
