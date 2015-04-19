namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataJudgments {
		public CallCreditDataJudgments() {
			JudgmentNocs = new List<CallCreditDataJudgmentsNocs>();
		}

		[PK]
		[NonTraversable]
		public long CallCreditDataJudgmentsID { get; set; }
		[FK("CallCreditData", "CallCreditDataID")]
		public long? CallCreditDataID { get; set; }

		[Length(164)]
		public string NameDetails { get; set; }
		public DateTime? Dob { get; set; }
		[Length(50)]
		public string CourtName { get; set; }
		public int? CourtType { get; set; }
		[Length(30)]
		public string CaseNumber { get; set; }
		[Length(10)]
		public string Status { get; set; }
		public int? Amount { get; set; }
		public DateTime? JudgmentDate { get; set; }
		public DateTime? DateSatisfied { get; set; }
		public bool? CurrentAddress { get; set; }
		public int? UnDeclaredAddressType { get; set; }
		[Length(440)]
		public string AddressValue { get; set; }

		[NonTraversable]
		public List<CallCreditDataJudgmentsNocs> JudgmentNocs { get; set; }

	}
}
