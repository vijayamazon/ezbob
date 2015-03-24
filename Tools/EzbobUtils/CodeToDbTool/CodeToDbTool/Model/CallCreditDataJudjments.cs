using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Utils.dbutils;

namespace CodeToDbTool.Model {
	class CallCreditDataJudjments {
		[PK]
		public long Id { get; set; }
		[FK("CallCreditData", "Id")]
		public long DataId { get; set; }
		[Length(164)]
		public string NameDetails { get; set; }
		public DateTime Dob { get; set; }
		[Length(50)]
		public string CourtName { get; set; }
		public int CourtType { get; set; }
		[Length(30)]
		public string CaseNumber { get; set; }
		[Length(10)]
		public string Status { get; set; }
		public int Amount { get; set; }
		public DateTime JudgmentDate { get; set; }
		public DateTime DateSatisfied { get; set; }
		public bool CurrentAddress { get; set; }
		public int UnDeclaredAddressType { get; set; }
		[Length(440)]
		public string AddressValue { get; set; }

	}
}
