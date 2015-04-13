namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataCreditScores {
		[PK]
		[NonTraversable]
		public long CallCreditDataCreditScoresID { get; set; }
		[FK("CallCreditData", "CallCreditDataID")]
		public long? CallCreditDataID { get; set; }

		public int? score { get; set; }
		public int? ScoreClass { get; set; }
		public int? Reason1 { get; set; }
		public int? Reason2 { get; set; }
		public int? Reason3 { get; set; }
		public int? Reason4 { get; set; }

	}
}
