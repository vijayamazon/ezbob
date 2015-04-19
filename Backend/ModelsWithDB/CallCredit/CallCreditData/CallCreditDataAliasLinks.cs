namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataAliasLinks {
		public CallCreditDataAliasLinks() {
			AliasLinkNocs = new List<CallCreditDataAliasLinksNocs>();
		}
		[PK]
		[NonTraversable]
		public long CallCreditDataAliasLinksID { get; set; }
		[FK("CallCreditData", "CallCreditDataID")]
		public long? CallCreditDataID { get; set; }

		public bool? Declared { get; set; }
		[Length(164)]
		public string NameBefore { get; set; }
		[Length(164)]
		public string Alias { get; set; }
		public DateTime? CreationDate { get; set; }
		public DateTime? LastConfDate { get; set; }
		[Length(60)]
		public string SupplierName { get; set; }
		[Length(10)]
		public string SupplierTypeCode { get; set; }

		[NonTraversable]
		public List<CallCreditDataAliasLinksNocs> AliasLinkNocs { get; set; }

	}
}
