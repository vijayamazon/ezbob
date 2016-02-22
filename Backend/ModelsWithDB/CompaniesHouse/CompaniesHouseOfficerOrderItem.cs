namespace Ezbob.Backend.ModelsWithDB.CompaniesHouse {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	[DataContract]
	public class CompaniesHouseOfficerOrderItem {
		[PK(true)]
		[DataMember]
		public int CompaniesHouseOfficerOrderItemID { get; set; }

		[FK("CompaniesHouseOfficerOrder", "CompaniesHouseOfficerOrderID")]
		[DataMember]
		public int CompaniesHouseOfficerOrderID { get; set; }
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
		public DateTime AppointedOn { get; set; }
		[DataMember]
		public string CountryOfResidence { get; set; }
		[DataMember]
		public int? DobDay { get; set; }
		[DataMember]
		public int? DobMonth { get; set; }
		[DataMember]
		public int? DobYear { get; set; }
		[DataMember]
		public string Link { get; set; }
		[DataMember]
		public string Name { get; set; }
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
		public CompaniesHouseOfficerAppointmentOrder AppointmentOrder { get; set; }

		[DataMember]
		[NonTraversable]
		public bool IsCustomer { get; set; }
	}//CompaniesHouseOfficerOrderItem
}//ns
