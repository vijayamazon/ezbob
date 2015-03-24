using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Utils.dbutils;

namespace CodeToDbTool.Model {
	class CallCreditDataBais {
		[PK]
		public long Id { get; set; }
		[FK("CallCreditData", "Id")]
		public long DataId { get; set; }
		public DateTime DischargeDate { get; set; }
		[Length(30)]
		public string LineOfBusiness { get; set; }
		[Length(50)]
		public string CourtName { get; set; }
		[Length(10)]
		public string CurrentStatus { get; set; }
		public int Amount { get; set; }
		[Length(10)]
		public string OrderType { get; set; }
		public DateTime OrderDate { get; set; }
		public int CaseYear { get; set; }
		[Length(20)]
		public string CaseRef { get; set; }
		[Length(164)]
		public string NameDetails { get; set; }
		[Length(60)]
		public string TradingName { get; set; }
		public DateTime Dob { get; set; }
		public bool CurrentAddress { get; set; }
		public int UnDeclaredAddressType { get; set; }
		[Length(440)]
		public string AddressValue { get; set; }
		[Length(10)]
		public string RestrictionType { get; set; }
		public DateTime Startdate { get; set; }
		public DateTime Enddate { get; set; }

	}
}
