using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Utils.dbutils;

namespace CodeToDbTool.Model {
	class CallCreditDataRtrNocs {
		[PK]
		public long Id { get; set; }
		[FK("CallCreditDataRtr", "Id")]
		public long RtrId { get; set; }
		[Length(10)]
		public string NoticeType { get; set; }
		[Length(30)]
		public string Refnum { get; set; }
		public DateTime DateRaised { get; set; }
		[Length(4000)]
		public string Text { get; set; }
		[Length(164)]
		public string NameDetails { get; set; }
		public bool CurrentAddress { get; set; }
		public int UnDeclaredAddressType { get; set; }
		[Length(440)]
		public string AddressValue { get; set; }

	}
}
