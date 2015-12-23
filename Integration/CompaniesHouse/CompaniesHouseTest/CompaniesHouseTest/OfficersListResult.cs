namespace CompaniesHouseTest {
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public class OfficersListResult {
		public int active_count { get; set; }
		public string etag { get; set; }
		public IList<Officer> items { get; set; }
		public int items_per_page { get; set; }
		public string kind { get; set; }
		public SelfLink links { get; set; }
		public int resigned_count { get; set; }
		public int start_index { get; set; }
		public int total_results { get; set; }
	}

	public class SelfLink {
		public string self { get; set; }
	}

	public class Officer {
		public OfficerAddress address { get; set; }
		public DateTime appointed_on { get; set; }
		public string country_of_residence { get; set; }
		public OfficerDateOfBirth date_of_birth { get; set; }
		public OfficerLink links { get; set; }
		public string name { get; set; }
		public string nationality { get; set; }
		public string occupation { get; set; }
		[JsonConverter(typeof(OfficerRoleConvertor))]
		public string officer_role { get; set; }
		public DateTime? resigned_on { get; set; }
	}

	public class OfficerAddress {
		public string address_line_1 { get; set; }
            public string address_line_2 { get; set; }
            public string care_of{ get; set; }
            public string country{ get; set; }
            public string locality{ get; set; }
            public string po_box{ get; set; }
            public string postal_code{ get; set; }
            public string premises{ get; set; }
            public string region{ get; set; }
	}

	public class OfficerDateOfBirth {
		public int? day { get; set; }
		public int month { get; set; }
        public int year { get; set; }
	}

	public class OfficerLink {
		public OfficerLinkOfficer officer { get; set; }
	}

	public class OfficerLinkOfficer {
		public string appointments { get; set; }
	}
}
