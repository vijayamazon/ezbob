namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataBais {
		public CallCreditDataBais() {
			BaiNocs = new List<CallCreditDataBaisNocs>();
		}

		[PK]
		[NonTraversable]
		public long CallCreditDataBaisID { get; set; }
		[FK("CallCreditData", "CallCreditDataID")]
		public long? CallCreditDataID { get; set; }

		public DateTime? DischargeDate { get; set; }
		[Length(30)]
		public string LineOfBusiness { get; set; }
		[Length(50)]
		public string CourtName { get; set; }
		[Length(10)]
		public string CurrentStatus { get; set; }
		public int? Amount { get; set; }
		[Length(10)]
		public string OrderType { get; set; }
		public DateTime? OrderDate { get; set; }
		public int? CaseYear { get; set; }
		[Length(20)]
		public string CaseRef { get; set; }
		[Length(164)]
		public string NameDetails { get; set; }
		[Length(60)]
		public string TradingName { get; set; }
		public DateTime? Dob { get; set; }
		public bool? CurrentAddress { get; set; }
		public int? UnDeclaredAddressType { get; set; }
		[Length(440)]
		public string AddressValue { get; set; }
		[Length(10)]
		public string RestrictionType { get; set; }
		public DateTime? Startdate { get; set; }
		public DateTime? Enddate { get; set; }

		[NonTraversable]
		public List<CallCreditDataBaisNocs> BaiNocs { get; set; }
	}
}
