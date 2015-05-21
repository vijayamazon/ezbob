namespace Ezbob.Backend.ModelsWithDB.Alibaba {
	using System;
	using System.Collections.Generic;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;


	public class AlibabaSeller {
		public AlibabaSeller() {
			Bank = new List<AlibabaSellerBank>();
		}

		[PK]
		[NonTraversable]
		public long SellerID { get; set; }
		[FK("AlibabaContract", "ContractID")]
		public long? ContractID { get; set; }
		[Length(100)]
		public string BusinessName { get; set; }
		[Length(100)]
		public string AliMemberId { get; set; }
		[Length(100)]
		public string Street1 { get; set; }
		[Length(100)]
		public string Street2 { get; set; }
		[Length(100)]
		public string City { get; set; }
		[Length(100)]
		public string State { get; set; }
		[Length(100)]
		public string Country { get; set; }
		[Length(100)]
		public string PostalCode { get; set; }
		[Length(100)]
		public string AuthRepFname { get; set; }
		[Length(100)]
		public string AuthRepLname { get; set; }
		[Length(100)]
		public string Phone { get; set; }
		[Length(100)]
		public string Fax { get; set; }
		[Length(100)]
		public string Email { get; set; }
		[Length(100)]
		public string GoldSupplierFlag { get; set; }
		[Length(100)]
		public string TenureWithAlibaba { get; set; }
		[Length(100)]
		public string BusinessStartDate { get; set; }
		public int? Size { get; set; }
		public int? suspiciousReportCountCounterfeitProduct { get; set; }
		public int? suspiciousReportCountRestrictedProhibitedProduct { get; set; }
		public int? suspiciousReportCountSuspiciousMember { get; set; }
		public int? ResponseRate { get; set; }
		public DateTime? GoldMemberStartDate { get; set; }
		public int? QuotationPerformance { get; set; }

		[NonTraversable]
		public List<AlibabaSellerBank> Bank { get; set; }
	}
}
