namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditEmail {

		[PK]
		[NonTraversable]
		public long CallCreditEmailID { get; set; }
		[FK("CallCredit", "CallCreditID")]
		public long? CallCreditID { get; set; }

		[Length(10)]
		public string EmailType { get; set; }
		[Length(100)]
		public string EmailAddress { get; set; }
	}
}
