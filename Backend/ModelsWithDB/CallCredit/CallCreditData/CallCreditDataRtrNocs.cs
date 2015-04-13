namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataRtrNocs {
		[PK]
		[NonTraversable]
		public long CallCreditDataRtrNocsID { get; set; }
		[FK("CallCreditDataRtr", "CallCreditDataRtrID")]
		public long? CallCreditDataRtrID { get; set; }

		[Length(10)]
		public string NoticeType { get; set; }
		[Length(30)]
		public string Refnum { get; set; }
		public DateTime? DateRaised { get; set; }
		[Length(4000)]
		public string Text { get; set; }
		[Length(164)]
		public string NameDetails { get; set; }
		public bool? CurrentAddress { get; set; }
		public int? UnDeclaredAddressType { get; set; }
		[Length(440)]
		public string AddressValue { get; set; }

	}
}
