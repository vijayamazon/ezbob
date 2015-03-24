using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Utils.dbutils;


namespace CodeToDbTool.Model {
	class CallCreditDataCreditScores {
		[PK]
		public long Id { get; set; }
		[FK("CallCreditData", "Id")]
		public long DataId { get; set; }
		public int score { get; set; }
		public int ScoreClass { get; set; }
		public int Reason1 { get; set; }
		public int Reason2 { get; set; }
		public int Reason3 { get; set; }
		public int Reason4 { get; set; }

	}
}
