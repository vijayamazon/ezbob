namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataAccsNocs {
		[PK]
		[NonTraversable]
		public long CallCreditDataAccsNocsID { get; set; }
		[FK("CallCreditDataAccs", "CallCreditDataAccsID")]
		public long? CallCreditDataAccsID { get; set; }

		[Length(10)]
		public string NoticeType { get; set; }
		[Length(30)]
		public string RefNum { get; set; }
		public DateTime? DateRaised { get; set; }
		[Length(7800)]
		public string Text { get; set; }
		[Length(164)]
		public string NameDetails { get; set; }
		public bool? CurrentAddress { get; set; }
		public int? UnDeclaredAddressType { get; set; }
		[Length(440)]
		public string AddressValue { get; set; }

	}
}
