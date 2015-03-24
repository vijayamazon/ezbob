using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Utils.dbutils;

namespace CodeToDbTool.Model {
	class CallCreditDataRtr {
		[PK]
		public long Id { get; set; }
		[FK("CallCreditData", "Id")]
		public long DataId { get; set; }
		[Length(164)]
		public string HolderName { get; set; }
		public DateTime Dob { get; set; }
		public bool CurrentAddress { get; set; }
		public int UnDeclaredAddressType { get; set; }
		[Length(440)]
		public string AddressValue { get; set; }
		public DateTime Updated { get; set; }
		[Length(164)]
		public string OrgTypeCode { get; set; }
		[Length(60)]
		public string OrgName { get; set; }
		[Length(20)]
		public string AccNum { get; set; }
		[Length(10)]
		public string AccSuffix { get; set; }
		[Length(10)]
		public string AccTypeCode { get; set; }
		public int Balance { get; set; }
		public int Limit { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		[Length(10)]
		public string AccStatusCode { get; set; }
		[Length(10)]
		public string RepayFreqCode { get; set; }
		public int NumOverdue { get; set; }
		public bool Rollover { get; set; }
		public bool CrediText { get; set; }
		public bool ChangePay { get; set; }
		public int NextPayAmount { get; set; }

	}
}
