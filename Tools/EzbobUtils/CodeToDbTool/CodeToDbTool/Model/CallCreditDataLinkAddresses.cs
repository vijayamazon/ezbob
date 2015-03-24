using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Utils.dbutils;

namespace CodeToDbTool.Model {
	class CallCreditDataLinkAddresses {
		[PK]
		public long Id { get; set; }
		[FK("CallCreditData", "Id")]
		public long DataID { get; set; }
		public int AddressID { get; set; }
		public bool Declared { get; set; }
		[Length(38)]
		public string NavLinkID { get; set; }
		public bool CurrentAddress { get; set; }
		public int UnDeclaredAddressType { get; set; }
		[Length(440)]
		public string AddressValue { get; set; }
	}
}
