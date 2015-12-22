namespace CompaniesHouseTest {
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public class AppointmentListResult {
		public OfficerDateOfBirth date_of_birth { get; set; }
		public string etag { get; set; }
		public bool is_corporate_officer { get; set; }
		public IList<Appointment> items { get; set; }
		public int items_per_page { get; set; }
		public string kind { get; set; }
		public SelfLink links { get; set; }
		public string name { get; set; }
		public int start_index { get; set; }
		public int total_results { get; set; }
	}

	public class Appointment {
		public OfficerAddress address { get; set; }
		public DateTime? appointed_before { get; set; }
		public DateTime appointed_on { get; set; }
		public AppointmentCompany appointed_to { get; set; }
		public string country_of_residence { get; set; }
		public IList<FormerName> former_names { get; set; }
		public Identification identification { get; set; }
		public bool is_pre_1992_appointment { get; set; }
		public AppointmentLink links { get; set; }
		public string name { get; set; }
		public NameElements name_elements { get; set; }
		public string nationality { get; set; }
		public string occupation { get; set; }
		[JsonConverter(typeof(OfficerRoleConvertor))]
		public string officer_role { get; set; }
		public DateTime? resigned_on { get; set; }

	}

	public class AppointmentCompany {
		public string company_name { get; set; }
		public string company_number { get; set; }
		[JsonConverter(typeof(CompanyStatusConvertor))]
		public string company_status { get; set; }
	}

	public class FormerName {
		public string forenames { get; set; }
		public string surname { get; set; }
	}

	public class Identification {
		[JsonConverter(typeof(IdentificationTypeConvertor))]
		public string identification_type { get; set; }
		public string legal_authority { get; set; }
		public string legal_form { get; set; }
		public string place_registered { get; set; }
		public string registration_number { get; set; }
	}

	public class AppointmentLink {
		public string company { get; set; }
	}

	public class NameElements {
		public string forename { get; set; }
		public string honours { get; set; }
		public string other_forenames { get; set; }
		public string surname { get; set; }
		public string title { get; set; }
	}
}
