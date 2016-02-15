namespace Ezbob.Backend.ModelsWithDB.CompaniesHouse {
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	[DataContract]
	public class CompaniesHouseOfficerAppointmentOrder {
		[PK(true)]
		[DataMember]
		public int CompaniesHouseOfficerAppointmentOrderID { get; set; }

		[FK("CompaniesHouseOfficerOrderItem", "CompaniesHouseOfficerOrderItemID")]
		[DataMember]
		public int CompaniesHouseOfficerOrderItemID { get; set; }
		[DataMember]
		public int? DobDay { get; set; }
		[DataMember]
		public int? DobMonth { get; set; }
		[DataMember]
		public int? DobYear { get; set; }
		[DataMember]
		public string Etag { get; set; }
		[DataMember]
		public bool IsCorporateOfficer { get; set; }
		[DataMember]
		public int ItemsPerPage { get; set; }
		[DataMember]
		public string Kind { get; set; }
		[DataMember]
		public string Link { get; set; }
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public int StartIndex { get; set; }
		[DataMember]
		public int TotalResults { get; set; }

		//--------------------//
		[DataMember]
		[NonTraversable]
		public IList<CompaniesHouseOfficerAppointmentOrderItem> Appointments { get; set; }
	}//CompaniesHouseOfficerAppointmentOrder
}//ns
