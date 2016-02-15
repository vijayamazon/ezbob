namespace Ezbob.Backend.Strategies.CompaniesHouse {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.CompaniesHouse;
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

		public void Fill(CompaniesHouseOfficerAppointmentOrder model) {
			model.Name = name;
			model.Etag = etag;
			model.IsCorporateOfficer= is_corporate_officer;
			model.ItemsPerPage= items_per_page;
			model.Kind = kind;
			model.Link = links != null ? links.self : null;
			model.StartIndex = start_index;
			model.TotalResults = total_results;
			if (model.Appointments == null) {
				model.Appointments = new List<CompaniesHouseOfficerAppointmentOrderItem>();
			}

			foreach (var item in items) {
				var appointmentModel = new CompaniesHouseOfficerAppointmentOrderItem();
				item.Fill(appointmentModel);
				model.Appointments.Add(appointmentModel);
			}//foreach
		}//Fill
	}//AppointmentListResult

	public class Appointment {
		public OfficerAddress address { get; set; }
		public DateTime? appointed_before { get; set; }
		public DateTime? appointed_on { get; set; }
		public AppointmentCompany appointed_to { get; set; }
		public string country_of_residence { get; set; }
		public IList<FormerName> former_names { get; set; } //TODO is necessary
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

		public void Fill(CompaniesHouseOfficerAppointmentOrderItem model) {
			model.Name = name;
			model.Nationality = nationality;
			model.AppointedOn = appointed_on;
			model.CountryOfResidence = country_of_residence;
			model.Link = links != null ? links.company : null;
			model.Occupation = occupation;
			model.OfficerRole = officer_role;
			model.ResignedOn = resigned_on;
			model.AppointedBefore = appointed_before;
			model.IsPre1992Appointment = is_pre_1992_appointment;

			if (identification != null) {
				model.IdentificationType = identification.identification_type;
				model.LegalAuthority = identification.legal_authority;
				model.LegalForm = identification.legal_form;
				model.PlaceRegistered = identification.place_registered;
				model.RegistrationNumber = identification.registration_number;
			}//if
			
			if (appointed_to != null) {
				model.CompanyName = appointed_to.company_name;
				model.CompanyNumber = appointed_to.company_number;
				model.CompanyStatus = appointed_to.company_status;
			}//if

			if (name_elements != null) {
				model.Forename = name_elements.forename;
				model.Honours = name_elements.honours;
				model.OtherForenames = name_elements.other_forenames;
				model.Surname = name_elements.surname;
				model.Title = name_elements.title;
			}//if

			if (address != null) {
				model.AddressLine1 = address.address_line_1;
				model.AddressLine2 = address.address_line_2;
				model.CareOf = address.care_of;
				model.Country = address.country;
				model.Locality = address.locality;
				model.PoBox = address.po_box;
				model.Postcode = address.postal_code;
				model.Premises = address.premises;
				model.Region = address.region;
			}//if
		}//Fill
	}//class Appointment

	public class AppointmentCompany {
		public string company_name { get; set; }
		public string company_number { get; set; }
		[JsonConverter(typeof(CompanyStatusConvertor))]
		public string company_status { get; set; }
	}//AppointmentCompany

	public class FormerName {
		public string forenames { get; set; }
		public string surname { get; set; }
	}//FormerName

	public class Identification {
		[JsonConverter(typeof(IdentificationTypeConvertor))]
		public string identification_type { get; set; }
		public string legal_authority { get; set; }
		public string legal_form { get; set; }
		public string place_registered { get; set; }
		public string registration_number { get; set; }
	}//Identification

	public class AppointmentLink {
		public string company { get; set; }
	}//AppointmentLink

	public class NameElements {
		public string forename { get; set; }
		public string honours { get; set; }
		public string other_forenames { get; set; }
		public string surname { get; set; }
		public string title { get; set; }
	}//NameElements
}//ns
