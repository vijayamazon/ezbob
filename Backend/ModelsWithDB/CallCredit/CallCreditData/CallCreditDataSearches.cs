namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataSearches {
		[PK]
		[NonTraversable]
		public long CallCreditDataSearchesID { get; set; }
		[FK("CallCreditData", "CallCreditDataID")]
		public long? CallCreditDataID { get; set; }
		[Length(38)]
		public string SearchRef { get; set; }
		[Length(10)]
		public string SearchOrgType { get; set; }
		[Length(50)]
		public string SearchOrgName { get; set; }
		[Length(50)]
		public string YourReference { get; set; }
		[Length(50)]
		public string SearchUnitName { get; set; }
		public bool? OwnSearch { get; set; }
		public bool? SubsequentEnquiry { get; set; }
		[Length(50)]
		public string UserName { get; set; }
		[Length(10)]
		public string SearchPurpose { get; set; }
		[Length(10)]
		public string CreditType { get; set; }
		public int? Balance { get; set; }
		public int? Term { get; set; }
		public bool? JointApplication { get; set; }
		public DateTime? SearchDate { get; set; }
		[Length(164)]
		public string NameDetailes { get; set; }
		public DateTime? Dob { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public bool? TpOptOut { get; set; }
		public bool? Transient { get; set; }
		public int? LinkType { get; set; }
		public bool? CurrentAddress { get; set; }
		public int? UnDeclaredAddressType { get; set; }
		[Length(440)]
		public string AddressValue { get; set; }

	}
}
