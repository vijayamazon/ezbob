namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataAddressLinks {
		public CallCreditDataAddressLinks() {
			AddressLinkNocs = new List<CallCreditDataAddressLinksNocs>();
		}

		[PK]
		[NonTraversable]
		public long CallCreditDataAddressLinksID { get; set; }
		[FK("CallCreditData", "CallCreditDataID")]
		public long? CallCreditDataID { get; set; }
		public DateTime? CreationDate { get; set; }
		public DateTime? LastConfDate { get; set; }
		public int? From { get; set; }
		public int? To { get; set; }
		[Length(60)]
		public string SupplierName { get; set; }
		[Length(10)]
		public string SupplierTypeCode { get; set; }

		[NonTraversable]
		public List<CallCreditDataAddressLinksNocs> AddressLinkNocs { get; set; }

	}
}
