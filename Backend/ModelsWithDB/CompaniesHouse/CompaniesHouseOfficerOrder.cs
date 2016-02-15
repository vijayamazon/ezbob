namespace Ezbob.Backend.ModelsWithDB.CompaniesHouse {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	[DataContract]
	public class CompaniesHouseOfficerOrder {
		public CompaniesHouseOfficerOrder() {
			Officers = new List<CompaniesHouseOfficerOrderItem>();
		}

		[PK(true)]
		[DataMember]
		public int CompaniesHouseOfficerOrderID { get; set; }
		[DataMember]
		public string CompanyRefNum { get; set; }
		[DataMember]
		public DateTime Timestamp { get; set; }
		[DataMember]
		public int ActiveCount { get; set; }
		[DataMember]
		public string Etag { get; set; }
		[DataMember]
		public int ItemsPerPage { get; set; }
		[DataMember]
		public string Kind { get; set; }
		[DataMember]
		public string Link { get; set; }
		[DataMember]
		public int ResignedCount { get; set; }
		[DataMember]
		public int StartIndex { get; set; }
		[DataMember]
		public int TotalResults { get; set; }
		[DataMember]
		public string Error { get; set; }
		//--------------------//
		[DataMember]
		[NonTraversable]
		public List<CompaniesHouseOfficerOrderItem> Officers { get; set; }
	}//CompaniesHouseOfficerOrder
}//ns
