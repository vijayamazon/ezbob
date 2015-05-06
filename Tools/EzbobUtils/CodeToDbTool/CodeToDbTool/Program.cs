namespace CodeToDbTool {
    using Ezbob.Utils.dbutils;
    using Ezbob.Backend.ModelsWithDB.NewLoan;

	class Program {
		static void Main(string[] args) {

			//Console.WriteLine(CodeToSql.GetCreateTable<CallCreditDataApplicant>());
			//Console.WriteLine(CodeToSql.GetCreateSp<CallCreditSearchData>());
           
		    CreateNewLoanTablesSps();
		}

	    private static void CreateNewLoanTablesSps()
	    {
	        CodeToFile.Folder = @"c:\temp\";
            CodeToFile.SaveFile<NL_CashRequests>();
            CodeToFile.SaveFile<NL_Decisions>();
            CodeToFile.SaveFile<NL_Offers>();
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


