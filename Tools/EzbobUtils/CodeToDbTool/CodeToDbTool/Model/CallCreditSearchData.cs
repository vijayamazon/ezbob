using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Utils.dbutils;

namespace CodeToDbTool.Model {

	public class CallCreditSearchData {
		[PK]
		public long Id { get; set; }
		[FK("MP_ServiceLog", "Id")]
		public long ServiceLogId { get; set; }
		[Length(4000)]
		public string PayLoadData { get; set; }
		[Length(50)]
		public string YourReference { get; set; }
		[Length(64)]
		public string Token { get; set; }
		[Length(5)]
		public string SchemaVersionCR { get; set; }
		public int DataSetsCR { get; set; }
		public bool Score { get; set; }
		[Length(10)]
		public string Purpose { get; set; }
		[Length(10)]
		public string CreditType { get; set; }
		public int BalorLim { get; set; }
		[Length(10)]
		public string Term { get; set; }
		public bool Transient { get; set; }
		public bool AutoSearch { get; set; }
		public int AutoSearchMaximum { get; set; }
		[Length(38)]
		public string SearchID { get; set; }
		[Length(1000)]
		public string CastInfo { get; set; }
		public int PSTV { get; set; }
		public int LS { get; set; }
		public DateTime SearchDate { get; set; }
		[Length(5)]
		public string SchemaVersionLR { get; set; }
		public int DataSetsLR { get; set; }
		[Length(38)]
		public string OrigSrchLRID { get; set; }
		[Length(38)]
		public string NavLinkID { get; set; }
		[Length(5)]
		public string SchemaVersionSR { get; set; }
		public int DataSetsSR { get; set; }
		[Length(38)]
		public string OrigSrchSRID { get; set; }


	}
}

