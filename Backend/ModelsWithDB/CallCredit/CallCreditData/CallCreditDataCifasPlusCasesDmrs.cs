﻿namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataCifasPlusCasesDmrs {
		[PK]
		[NonTraversable]
		public long CallCreditDataCifasPlusCasesDmrsID { get; set; }
		[FK("CallCreditDataCifasPlusCases", "CallCreditDataCifasPlusCasesID")]
		public long? CallCreditDataCifasPlusCasesID { get; set; }

		public int dmr { get; set; }

	}
}