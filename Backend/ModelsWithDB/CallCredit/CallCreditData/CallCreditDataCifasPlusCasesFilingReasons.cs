namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataCifasPlusCasesFilingReasons {
		[PK]
		[NonTraversable]
		public long CallCreditDataCifasPlusCasesFilingReasonsID { get; set; }
		[FK("CallCreditDataCifasPlusCases", "CallCreditDataCifasPlusCasesID")]
		public long? CallCreditDataCifasPlusCasesID { get; set; }

		[Length(10)]
		public string FilingReason { get; set; }

	}
}
