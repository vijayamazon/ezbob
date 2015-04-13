namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataAddressConfs {
		public CallCreditDataAddressConfs() {
			Residents = new List<CallCreditDataAddressConfsResidents>();
		}

		[PK]
		[NonTraversable]
		public long CallCreditDataAddressConfsID { get; set; }
		[FK("CallCreditData", "CallCreditDataID")]
		public long? CallCreditDataID { get; set; }

		public bool? PafValid { get; set; }
		public bool? OtherResidents { get; set; }
		public bool? CurrentAddress { get; set; }
		public int? UnDeclaredAddressType { get; set; }
		[Length(440)]
		public string AddressValue { get; set; }

		public List<CallCreditDataAddressConfsResidents> Residents { get; set; }

	}
}
