namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataCifasPlusCasesSubjects {
		[PK]
		[NonTraversable]
		public long CallCreditDataCifasPlusCasesSubjectsID { get; set; }
		[FK("CallCreditDataCifasPlusCases", "CallCreditDataCifasPlusCasesID")]
		public long? CallCreditDataCifasPlusCasesID { get; set; }

		[Length(164)]
		public string PersonName { get; set; }
		public DateTime? PersonDob { get; set; }
		[Length(70)]
		public string CompanyName { get; set; }
		[Length(8)]
		public string CompanyNumber { get; set; }
		[Length(20)]
		public string HomeTelephone { get; set; }
		[Length(20)]
		public string MobileTelephone { get; set; }
		[Length(60)]
		public string Email { get; set; }
		[Length(10)]
		public string SubjectRole { get; set; }
		[Length(10)]
		public string SubjectRoleQualifier { get; set; }
		[Length(10)]
		public string AddressType { get; set; }
		public bool? CurrentAddress { get; set; }
		public int? UndeclaredAddressType { get; set; }
		[Length(440)]
		public string AddressValue { get; set; }

	}
}
