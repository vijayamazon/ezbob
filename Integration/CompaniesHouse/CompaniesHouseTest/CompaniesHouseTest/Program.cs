
namespace CompaniesHouseTest {
	using System;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using Newtonsoft.Json;

	class Program {
		private static void Main(string[] args) {
			
			string apiKey = "40CdyyGYerf2q-cKZwgN0xVdlrwXNVhxwiMVxpG-";

			HttpClient httpClient = new HttpClient();
			httpClient.BaseAddress = new Uri("https://api.companieshouse.gov.uk/");
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(apiKey)));

			int startIndex = 0;
			const int itemsPerPage = 5;
			bool retrieve = true;
			do {

				//Get officers of company by ref num
				string companyRefNum = "03972564";
				var result = httpClient.GetAsync(string.Format("company/{0}/officers/?items_per_page={1}&start_index={2}", companyRefNum, itemsPerPage, startIndex)).Result.Content.ReadAsStringAsync().Result;
				var resultModel = JsonConvert.DeserializeObject<OfficersListResult>(result);
				if (resultModel.total_results > (startIndex + itemsPerPage)) {
					startIndex += itemsPerPage;
				} else {
					retrieve = false;
				}
			} while (retrieve);


			startIndex = 0;
			retrieve = true;
			do {
				//Get appointments by officer ref num
				var officerRefNum = "/officers/2Z5vDM3ulIo2gYa7M8sgl_FBd6o/appointments";
				var result2 = httpClient.GetAsync(string.Format("{0}/?items_per_page={1}&start_index={2}", officerRefNum, itemsPerPage, startIndex))
					.Result.Content.ReadAsStringAsync()
					.Result;
				var result2Model = JsonConvert.DeserializeObject<AppointmentListResult>(result2);

				if (result2Model.total_results > (startIndex + itemsPerPage)) {
					startIndex += itemsPerPage;
				} else {
					retrieve = false;
				}
			} while (retrieve);
		}
	}
}


//result "{\"items\":[{\"links\":{\"officer\":{\"appointments\":\"\\/officers\\/vMivDnTintbrtCCl42WplsTLRRM\\/appointments\"}},\"name\":\"PANDEY, Pratibha\",\"appointed_on\":\"2007-03-20\",\"address\":{\"premises\":\"Lynton House\",\"address_line_1\":\"7-12 Tavistock Square\",\"country\":\"England\",\"postal_code\":\"WC1H 9LT\",\"locality\":\"London\"},\"officer_role\":\"secretary\"},{\"address\":{\"locality\":\"London\",\"postal_code\":\"WC1H 9LT\",\"country\":\"England\",\"address_line_1\":\"7-12 Tavistock Square\",\"premises\":\"Lynton House\"},\"occupation\":\"Businessman\",\"officer_role\":\"director\",\"nationality\":\"British\",\"links\":{\"officer\":{\"appointments\":\"\\/officers\\/1t13BKJFE1XYSoeCqJVckOnnSwk\\/appointments\"}},\"name\":\"LOPEZ, Dario\",\"date_of_birth\":{\"month\":11,\"year\":1990},\"country_of_residence\":\"United Kingdom\",\"appointed_on\":\"2007-03-20\"},{\"address\":{\"postal_code\":\"CM14 4SX\",\"locality\":\"Brentwood\",\"care_of\":\"PLAN A FINANCIALS\",\"address_line_1\":\"Weald Road\",\"premises\":\"Leigh House\",\"country\":\"England\",\"region\":\"Essex\"},\"officer_role\":\"director\",\"occupation\":\"Head Of Operation\",\"nationality\":\"British\",\"links\":{\"officer\":{\"appointments\":\"\\/officers\\/uGRMz8o47XIXprXPU9XzC3xBAd4\\/appointments\"}},\"name\":\"PANDEY, Pratibha\",\"country_of_residence\":\"United Kingdom\",\"date_of_birth\":{\"year\":1988,\"month\":10},\"appointed_on\":\"2012-07-05\"},{\"nationality\":\"British\",\"occupation\":\"Director\",\"officer_role\":\"director\",\"address\":{\"region\":\"Essex\",\"country\":\"United Kingdom\",\"premises\":\"36 Redwood Drive\",\"postal_code\":\"SS15 4AF\",\"locality\":\"Basildon\"},\"resigned_on\":\"2015-05-12\",\"appointed_on\":\"2015-01-31\",\"date_of_birth\":{\"month\":9,\"year\":1982},\"country_of_residence\":\"United Kingdom\",\"name\":\"HARRIS, Tony David\",\"links\":{\"officer\":{\"appointments\":\"\\/officers\\/9fa9OSNLYAwKUXCzrrtf3T2luVM\\/appointments\"}}},{\"occupation\":\"Director\",\"officer_role\":\"director\",\"nationality\":\"British\",\"address\":{\"address_line_1\":\"Eastways\",\"premises\":\"2 Rosewood Business Park\",\"region\":\"Essex\",\"country\":\"United Kingdom\",\"postal_code\":\"CM8 3AA\",\"locality\":\"Witham\"},\"resigned_on\":\"2015-05-12\",\"date_of_birth\":{\"month\":3,\"year\":1979},\"country_of_residence\":\"United Kingdom\",\"appointed_on\":\"2015-01-31\",\"links\":{\"officer\":{\"appointments\":\"\\/officers\\/aXntnkn5l1yeNRN3f8NTkpU5zfI\\/appointments\"}},\"name\":\"HYETT, David\"}],\"links\":{\"self\":\"\\/company\\/06173420\\/appointments\"},\"kind\":\"officer-list\",\"active_count\":3,\"resigned_count\":2,\"etag\":\"760cc33a62ca78b6ec3e86324ee0825539b0f094\",\"start_index\":0,\"items_per_page\":35,\"total_results\":5}"	string

//result 2 {"start_index":0,"is_corporate_officer":false,"items_per_page":35,"total_results":3,"kind":"personal-appointment","items":[{"appointed_on":"2010-12-29","address":{"address_line_1":"Samuel Street","locality":"London","premises":"95","country":"United Kingdom","postal_code":"SE18 5LF"},"nationality":"British","appointed_to":{"company_status":"dissolved","company_number":"07479028","company_name":"PRINT 3D LIMITED"},"occupation":"Director","links":{"company":"\/company\/07479028"},"officer_role":"director","name_elements":{"surname":"LOPEZ","forename":"Dario","title":"Mr"},"country_of_residence":"United Kingdom","name":"Dario LOPEZ"},{"address":{"address_line_2":"Woolwich, London","locality":"London","premises":"95","address_line_1":"Samuel Street","country":"United Kingdom","postal_code":"SE18 5LF"},"appointed_on":"2008-10-31","name_elements":{"forename":"Dario","surname":"LOPEZ","title":"Mr"},"links":{"company":"\/company\/05256346"},"officer_role":"director","country_of_residence":"United Kingdom","name":"Dario LOPEZ","nationality":"British","appointed_to":{"company_name":"LONDON MAGIC STORE LTD.","company_number":"05256346","company_status":"dissolved"},"occupation":"Business Owner"},{"links":{"company":"\/company\/06173420"},"name_elements":{"forename":"Dario","surname":"LOPEZ","title":"Mr"},"officer_role":"director","name":"Dario LOPEZ","country_of_residence":"United Kingdom","nationality":"British","occupation":"Businessman","appointed_to":{"company_name":"AIODISTRIBUTION LTD","company_number":"06173420","company_status":"active"},"address":{"locality":"London","premises":"Lynton House","address_line_1":"7-12 Tavistock Square","country":"England","postal_code":"WC1H 9LT"},"appointed_on":"2007-03-20"}],"links":{"self":"\/officers\/1t13BKJFE1XYSoeCqJVckOnnSwk\/appointments"},"name":"Dario LOPEZ","date_of_birth":{"month":11,"year":1990},"etag":"9946df2faa87e5b8a7f63e41a8db46604ccf6688"}
