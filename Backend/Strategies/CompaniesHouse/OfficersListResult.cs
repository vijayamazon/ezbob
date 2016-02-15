namespace Ezbob.Backend.Strategies.CompaniesHouse {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.CompaniesHouse;
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

		public void Fill(CompaniesHouseOfficerOrder model) {
			model.ActiveCount = active_count;
			model.Etag = etag;
			model.ItemsPerPage = items_per_page;
			model.Link = (links != null) ? links.self : null;
			model.Kind = kind;
			model.ResignedCount = resigned_count;
			model.StartIndex = start_index;
			model.TotalResults = total_results;

			if (model.Officers == null) {
				model.Officers = new List<CompaniesHouseOfficerOrderItem>();
			}
		}//Fill
	}//class OfficersListResult

	public class SelfLink {
		public string self { get; set; }
	}//SelfLink

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

		public void Fill(CompaniesHouseOfficerOrderItem model) {
			model.Name = name;
			model.Nationality = nationality;
			model.AppointedOn = appointed_on;
			model.CountryOfResidence = country_of_residence;
			model.Link = links != null && links.officer != null ? links.officer.appointments : null;
			model.Occupation = occupation;
			model.OfficerRole = officer_role;
			model.ResignedOn = resigned_on;

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

			if (date_of_birth != null) {
				model.DobDay = date_of_birth.day;
				model.DobMonth = date_of_birth.month;
				model.DobYear = date_of_birth.year;
			}//if

			model.AppointmentOrder = new CompaniesHouseOfficerAppointmentOrder();
		}//Fill
	}//Officer

	public class OfficerAddress {
		public string address_line_1 { get; set; }
		public string address_line_2 { get; set; }
		public string care_of { get; set; }
		public string country { get; set; }
		public string locality { get; set; }
		public string po_box { get; set; }
		public string postal_code { get; set; }
		public string premises { get; set; }
		public string region { get; set; }
	}//class OfficerAddress

	public class OfficerDateOfBirth {
		public int? day { get; set; }
		public int month { get; set; }
		public int year { get; set; }
	}// class OfficerDateOfBirth

	public class OfficerLink {
		public OfficerLinkOfficer officer { get; set; }
	}//class OfficerLink

	public class OfficerLinkOfficer {
		public string appointments { get; set; }
	}// class OfficerLinkOfficer
}//ns
