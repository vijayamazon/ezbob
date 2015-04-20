namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils; 

	public class CallCreditDataAccsHistory {
		[PK]
		[NonTraversable]		
		public long CallCreditDataAccsHistoryID { get; set; }
		[FK("CallCreditDataAccs", "CallCreditDataAccsID")]
		public long? CallCreditDataAccsID { get; set; }

		public DateTime? M { get; set; }
		public int? Bal { get; set; }
		public int? CreditLimit { get; set; }
		[Length(10)]
		public string Acc { get; set; }
		[Length(10)]
		public string Pay { get; set; }
		public int? StmtBal { get; set; }
		public int? PayAmt { get; set; }
		public int? CashAdvCount { get; set; }
		public int? CashAdvTotal { get; set; }

	}
}
