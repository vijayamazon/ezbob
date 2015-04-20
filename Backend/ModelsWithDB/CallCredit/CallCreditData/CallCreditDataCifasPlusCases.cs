namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataCifasPlusCases {
		public CallCreditDataCifasPlusCases() {
			Dmrs = new List<CallCreditDataCifasPlusCasesDmrs>();
			FilingReasons = new List<CallCreditDataCifasPlusCasesFilingReasons>();
			CifasPlusCaseNocs = new List<CallCreditDataCifasPlusCasesNocs>();
			Subjects = new List<CallCreditDataCifasPlusCasesSubjects>();
		}
		[PK]
		[NonTraversable]
		public long CallCreditDataCifasPlusCasesID { get; set; }
		[FK("CallCreditData", "CallCreditDataID")]
		public long? CallCreditDataID { get; set; }

		public int? CaseId { get; set; }
		public int? OwningMember { get; set; }
		public int? ManagingMember { get; set; }
		[Length(10)]
		public string CaseType { get; set; }
		[Length(10)]
		public string ProductCode { get; set; }
		[Length(10)]
		public string Facility { get; set; }
		public DateTime? SupplyDate { get; set; }
		public DateTime? ExpiryDate { get; set; }
		public DateTime? ApplicationDate { get; set; }

		[NonTraversable]
		public List<CallCreditDataCifasPlusCasesDmrs> Dmrs { get; set; }
		[NonTraversable]
		public List<CallCreditDataCifasPlusCasesFilingReasons> FilingReasons { get; set; }
		[NonTraversable]
		public List<CallCreditDataCifasPlusCasesNocs> CifasPlusCaseNocs { get; set; }
		[NonTraversable]
		public List<CallCreditDataCifasPlusCasesSubjects> Subjects { get; set; }

	}
}
