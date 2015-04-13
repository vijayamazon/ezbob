namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditTelephone {

		[PK]
		[NonTraversable]
		public long CallCreditTelephoneID { get; set; }
		[FK("CallCredit", "CallCreditID")]
		public long? CallCreditID { get; set; }
		
		[Length(10)]
		public string TelephoneType { get; set; }
		[Length(5)]
		public string STD { get; set; }
		[Length(11)]
		public string PhoneNumber { get; set; }
		[Length(5)]
		public string Extension { get; set; }
	}
}
