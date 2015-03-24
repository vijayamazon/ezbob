using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Utils.dbutils;

namespace CodeToDbTool.Model {

	public class CallCreditDataApplicantNames {
		[PK]
		public long Id { get; set; }
		[FK("CallCreditDataApplicant", "Id")]
		public long ApplicantId { get; set; }
		[Length(30)]
		public string Title { get; set; }
		[Length(30)]
		public string Forename { get; set; }
		[Length(40)]
		public string OtherNames { get; set; }
		[Length(30)]
		public string Surname { get; set; }
		[Length(30)]
		public string Suffix { get; set; }
	}
}
