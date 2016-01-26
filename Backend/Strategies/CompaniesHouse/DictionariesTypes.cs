namespace Ezbob.Backend.Strategies.CompaniesHouse {
	using System.Collections.Generic;

	public static class Dictionaries {
		public static Dictionary<string, string> serviceErrors = new Dictionary<string, string> {
			{"access-denied" , "Access denied"},
			{"company-profile-not-found" , "Company profile not found"},
			{"company-insolvencies-not-found" , "Company insolvencies not found"},
			{"invalid-authorization-header" , "Invalid authorization header"},
			{"invalid-http-method" , "Access denied for HTTP method {method}"},
			{"invalid-client-id" , "Invalid client ID"},
			{"no-json-provided" , "No JSON payload provided"},
			{"not-authorised-for-company" , "Not authorised to file for this company"},
			{"transaction-not-open" , "Transaction is not open"},
			{"transaction-does-not-exist" , "Transaction does not exist"},
			{"user-transactions-not-found" , "No transactions found for this user"},
			{"unauthorised" , "Unauthorised"},
		};

		public static Dictionary<string, string> officer_role = new Dictionary<string, string> {
			{"cic-manager", "CIC Manager"},
			{"corporate-director", "Director"},
			{"corporate-llp-designated-member", "LLP Designated Member"},
			{"corporate-llp-member", "LLP Member"},
			{"corporate-manager-of-an-eeig", "Manager of an EEIG"},
			{"corporate-member-of-a-management-organ", "Member of a Management Organ"},
			{"corporate-member-of-a-supervisory-organ", "Member of a Supervisory Organ"},
			{"corporate-member-of-an-administrative-organ", "Member of an Administrative Organ"},
			{"corporate-nominee-director", "Nominee Director"},
			{"corporate-nominee-secretary", "Nominee Secretary"},
			{"corporate-secretary", "Secretary"},
			{"director", "Director"},
			{"general-partner-in-a-limited-partnership", "General Partner in a Limited Partnership"},
			{"judicial-factor", "Judicial Factor"},
			{"limited-partner-in-a-limited-partnership", "Limited Partner in a Limited Partnership"},
			{"llp-designated-member", "LLP Designated Member"},
			{"llp-member", "LLP Member"},
			{"manager-of-an-eeig", "Manager of an EEIG"},
			{"member-of-a-management-organ", "Member of a Management Organ"},
			{"member-of-a-supervisory-organ", "Member of a Supervisory Organ"},
			{"member-of-an-administrative-organ", "Member of an Administrative Organ"},
			{"nominee-director", "Nominee Director"},
			{"nominee-secretary", "Nominee Secretary"},
			{"person-authorised-to-accept", "Person Authorised to Accept"},
			{"person-authorised-to-represent", "Person Authorised to Represent"},
			{"person-authorised-to-represent-and-accept", "Person Authorised to Represent and Accept"},
			{"receiver-and-manager", "Receiver and Manager"},
			{"secretary", "Secretary"}
		};

		public static Dictionary<string,string> identification_type = new Dictionary<string, string> {
				{"non-eea" , "Non European Economic Area"},
				{"eea" , "European Economic Area"}
			};
	
		public static Dictionary<string,string> company_status = new Dictionary<string, string> {
				{ "active" , "Active"},
				{ "dissolved" , "Dissolved"},
				{ "liquidation" , "Liquidation"},
				{ "receivership" , "Receivership"},
				{ "converted-closed" , "Converted / Closed"},
				{ "voluntary-arrangement" , "Voluntary Arrangement"},
				{ "insolvency-proceedings" , "Insolvency Proceedings"},
				{ "administration" , "In Administration"}
			};
	}
}
