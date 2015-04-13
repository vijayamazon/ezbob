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
			using (StreamWriter file1 = new System.IO.StreamWriter(@"c:\temp1\BuildDB.txt", true)) {
                file1.WriteLine(CodeToSql.GetCreateTable<CreditSafeNonLtdRatings>());
			};

            using (StreamWriter file2 = new System.IO.StreamWriter(@"c:\temp1\SaveCreditSafeNonLtdRatings.sql", true))
            {
                file2.WriteLine(CodeToSql.GetCreateSp<CreditSafeNonLtdRatings>());
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


