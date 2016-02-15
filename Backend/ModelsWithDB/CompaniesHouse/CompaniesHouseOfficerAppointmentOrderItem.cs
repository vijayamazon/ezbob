namespace Ezbob.Backend.ModelsWithDB.CompaniesHouse {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	[DataContract]
	public class CompaniesHouseOfficerAppointmentOrderItem {
		[PK(true)]
		[DataMember]
		public int CompaniesHouseOfficerAppointmetOrderItemID { get; set; }

		[FK("CompaniesHouseOfficerAppointmentOrder", "CompaniesHouseOfficerAppointmentOrderID")]
		[DataMember]
		public int CompaniesHouseOfficerAppointmentOrderID { get; set; }

		//Address
		[DataMember]
		public string AddressLine1 { get; set; }
		[DataMember]
		public string AddressLine2 { get; set; }
		[DataMember]
		public string CareOf { get; set; }
		[DataMember]
		public string Country { get; set; }
		[DataMember]
		public string Locality { get; set; }
		[DataMember]
		public string PoBox { get; set; }
		[DataMember]
		public string Postcode { get; set; }
		[DataMember]
		public string Premises { get; set; }
		[DataMember]
		public string Region { get; set; }
		[DataMember]
		public DateTime? AppointedBefore { get; set; }
		[DataMember]
		public DateTime? AppointedOn { get; set; }

		//Company
		[DataMember]
		public string CompanyName { get; set; }
		[DataMember]
		public string CompanyNumber { get; set; }
		[DataMember]
		public string CompanyStatus { get; set; }
		
		[DataMember]
		public string CountryOfResidence { get; set; }

		//Identification
		[DataMember]
		public string IdentificationType { get; set; }
		[DataMember]
		public string LegalAuthority { get; set; }
		[DataMember]
		public string LegalForm { get; set; }
		[DataMember]
		public string PlaceRegistered { get; set; }
		[DataMember]
		public string RegistrationNumber { get; set; }

		[DataMember]
		public bool IsPre1992Appointment { get; set; }
		[DataMember]
		public string Link { get; set; }
		[DataMember]
		public string Name { get; set; }

		//Name
		[DataMember]
		public string Forename { get; set; }
		[DataMember]
		public string Honours { get; set; }
		[DataMember]
		public string OtherForenames { get; set; }
		[DataMember]
		public string Surname { get; set; }
		[DataMember]
		public string Title { get; set; }
		[DataMember]
		public string Nationality { get; set; }
		[DataMember]
		public string Occupation { get; set; }
		[DataMember]
		public string OfficerRole { get; set; }
		[DataMember]
		public DateTime? ResignedOn { get; set; }

		//--------------------//
		[DataMember]
		[NonTraversable]
		public bool IsCustomer { get; set; }
		
	}//CompaniesHouseOfficerAppointmentOrderItem
}//ns
