using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Callcredit.CRBSB;
using CodeToDbTool.Model;
using Ezbob.Utils.dbutils;
using System.IO;


namespace CodeToDbTool {
	class Program {
		static void Main(string[] args) {

			//Console.WriteLine(CodeToSql.GetCreateTable<CallCreditDataApplicant>());
			//Console.WriteLine(CodeToSql.GetCreateSp<CallCreditSearchData>());
			using (StreamWriter file1 = new System.IO.StreamWriter(@"c:\temp\BuildDB.txt", true)) {
				file1.WriteLine(CodeToSql.GetCreateTable<CallCreditDataLinkAddresses>());
			};

			using (StreamWriter file2 = new System.IO.StreamWriter(@"c:\temp\BuildCP.txt", true)) {
				file2.WriteLine(CodeToSql.GetCreateSp<CallCreditDataLinkAddresses>());
			}




		}
	}

	public class ExampleDbTable {
		[PK]
		public long Id { get; set; }
		[FK("ForeignTable", "Id")]
		public long? ForeignId { get; set; }

		public string Field { get; set; }


	}
}


