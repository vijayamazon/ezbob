namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataCifasFiling {
		public CallCreditDataCifasFiling() {
			CifasFilingNocs = new List<CallCreditDataCifasFilingNocs>();
		}

		[PK]
		[NonTraversable]
		public long CallCreditDataCifasFilingID { get; set; }
		[FK("CallCreditData", "CallCreditDataID")]
		public long? CallCreditDataID { get; set; }
		[Length(164)]
		public string PersonName { get; set; }
		public DateTime? Dob { get; set; }
		public bool? CurrentAddressP { get; set; }
		public int? UnDeclaredAddressTypeP { get; set; }
		[Length(440)]
		public string AddressValueP { get; set; }
		[Length(8)]
		public string CompanyNumber { get; set; }
		[Length(70)]
		public string CompanyName { get; set; }
		public bool? CurrentAddressC { get; set; }
		public int? UnDeclaredAddressTypeC { get; set; }
		[Length(440)]
		public string AddressValueC { get; set; }
		public int? MemberNumber { get; set; }
		[Length(6)]
		public string CaseReferenceNo { get; set; }
		[Length(100)]
		public string MemberName { get; set; }
		[Length(10)]
		public string ProductCode { get; set; }
		[Length(10)]
		public string FraudCategory { get; set; }
		[Length(150)]
		public string ProductDesc { get; set; }
		[Length(50)]
		public string FraudDesc { get; set; }
		public DateTime? InputDate { get; set; }
		public DateTime? ExpiryDate { get; set; }
		[Length(10)]
		public string TransactionType { get; set; }

		public List<CallCreditDataCifasFilingNocs> CifasFilingNocs { get; set; }
	}
}
