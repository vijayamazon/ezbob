namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataAddressConfsResidents {
		public CallCreditDataAddressConfsResidents() {
			ErHistory = new List<CallCreditDataAddressConfsResidentsErHistory>();
			ResidentNocs = new List<CallCreditDataAddressConfsResidentsNocs>();
		}

		[PK]
		[NonTraversable]
		public long CallCreditDataAddressConfsResidentsID { get; set; }
		[FK("CallCreditDataAddressConfs", "CallCreditDataAddressConfsID")]
		public long? CallCreditDataAddressConfsID { get; set; }
		[Length(10)]
		public string MatchType { get; set; }
		public bool? CurrentName { get; set; }
		public bool? DeclaredAlias { get; set; }
		[Length(164)]
		public string NameDetails { get; set; }
		[Length(30)]
		public string Duration { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public int? ErValid { get; set; }

		public List<CallCreditDataAddressConfsResidentsErHistory> ErHistory { get; set; }
		public List<CallCreditDataAddressConfsResidentsNocs> ResidentNocs { get; set; }

	}
}
