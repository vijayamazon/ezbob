namespace Ezbob.Backend.ModelsWithDB.Alibaba {
	using System;
	using System.Collections.Generic;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class AlibabaSellerBank {

		[PK]
		[NonTraversable]
		public long SellerBankID { get; set; }
		[FK("AlibabaSeller", "SellerID")]
		public long SellerID { get; set; }
		[Length(100)]
		public string BeneficiaryBank { get; set; }
		[Length(100)]
		public string StreetAddr1 { get; set; }
		[Length(100)]
		public string StreetAddr2 { get; set; }
		[Length(100)]
		public string City { get; set; }
		[Length(100)]
		public int? State { get; set; }
		[Length(100)]
		public string Country { get; set; }
		[Length(100)]
		public string PostalCode { get; set; }
		[Length(100)]
		public string SwiftCode { get; set; }
		[Length(100)]
		public string AccountNumber { get; set; }
		[Length(100)]
		public string WireInstructions { get; set; }
	}
}
