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
            CodeToFile.SaveFile<NL_LoanAgreements>();
            CodeToFile.SaveFile<NL_LoanFeePayments>();
            CodeToFile.SaveFile<NL_BlendedLoans>();
            CodeToFile.SaveFile<NL_FundTransfers>();
            CodeToFile.SaveFile<NL_LoanFees>();
            CodeToFile.SaveFile<NL_LoanHistory>();
            CodeToFile.SaveFile<NL_BlendedOffers>();
            CodeToFile.SaveFile<NL_LoanLegals>();
            CodeToFile.SaveFile<NL_LoanLienLinks>();
            CodeToFile.SaveFile<NL_LoanRollovers>();
            CodeToFile.SaveFile<NL_PaypointTransactions>();
            CodeToFile.SaveFile<NL_PacnetTransactions>();
            CodeToFile.SaveFile<NL_Payments>();
            CodeToFile.SaveFile<NL_LoanSchedulePayments>();
            CodeToFile.SaveFile<NL_LoanSchedules>();
            CodeToFile.SaveFile<NL_LoanStates>();
            CodeToFile.SaveFile<NL_Loans>();
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


