namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditApplicantNames {
		[PK]
		[NonTraversable]
		public long CallCreditApplicantNamesID { get; set; }
		[FK("CallCredit", "CallCreditID")]
		public long? CallCreditID { get; set; }
		[Length(30)]
		public string Title { get; set; }
		[Length(30)]
		public string Forename { get; set; }
		[Length(40)]
		public string OtherNames { get; set; }
		[Length(30)]
		public string Surname { get; set; }
		[Length(30)]
		public string Suffix { get; set; }
	}
}
