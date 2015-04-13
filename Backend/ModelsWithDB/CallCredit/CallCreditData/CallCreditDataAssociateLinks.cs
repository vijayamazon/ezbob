﻿namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataAssociateLinks {
		public CallCreditDataAssociateLinks() {
			AssociateLinkNocs = new List<CallCreditDataAssociateLinksNocs>();
		}

		[PK]
		[NonTraversable]
		public long CallCreditDataAssociateLinksID { get; set; }
		[FK("CallCreditData", "CallCreditDataID")]
		public long? CallCreditDataID { get; set; }

		public bool? DeclaredAddress { get; set; }
		public int? OiaID { get; set; }
		[Length(38)]
		public string NavLinkID { get; set; }
		[Length(164)]
		public string AssociateName { get; set; }
		public DateTime? CreationDate { get; set; }
		public DateTime? LastConfDate { get; set; }
		[Length(60)]
		public string SupplierName { get; set; }
		[Length(10)]
		public string SupplierTypeCode { get; set; }

		public List<CallCreditDataAssociateLinksNocs> AssociateLinkNocs { get; set; }

	}
}