namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataAddressConfsResidentsErHistory {
		public CallCreditDataAddressConfsResidentsErHistory() {
			ErHistoryNocs = new List<CallCreditDataAddressConfsResidentsErHistoryNocs>();
		}

		[PK]
		[NonTraversable]
		public long CallCreditDataAddressConfsResidentsErHistoryID { get; set; }
		[FK("CallCreditDataAddressConfsResidents", "CallCreditDataAddressConfsResidentsId")]
		public long? CallCreditDataAddressConfsResidentsID { get; set; }

		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public bool? Optout { get; set; }
		public bool? RollingRoll { get; set; }

		public List<CallCreditDataAddressConfsResidentsErHistoryNocs> ErHistoryNocs { get; set; }

	}
}
