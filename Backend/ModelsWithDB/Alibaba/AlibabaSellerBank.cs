namespace Ezbob.Backend.ModelsWithDB.Alibaba {
	using System;
	using System.Collections.Generic;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class AlibabaSellerBank {

		[PK(true)]
		[NonTraversable]
		public int SellerBankID { get; set; }
		[FK("AlibabaSeller", "SellerID")]
		public int SellerID { get; set; }
		[Length(100)]
		public string BeneficiaryBank { get; set; }
		[Length(100)]
		public string StreetAddr1 { get; set; }
		[Length(100)]
		public string StreetAddr2 { get; set; }
		[Length(100)]
		public string City { get; set; }
		[Length(100)]
		public string State { get; set; }
		[Length(100)]
		public string Country { get; set; }
		[Length(100)]
		public string PostalCode { get; set; }
        [Length(100)]
		public string SwiftCode { get; set; }
		public int? AccountNumber { get; set; }
		[Length(100)]
		public string WireInstructions { get; set; }
	}
}
