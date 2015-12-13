namespace CodeToDbTool {
    using Ezbob.Utils.dbutils;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;

	class Program {
		static void Main(string[] args) {

			//Console.WriteLine(CodeToSql.GetCreateTable<CallCreditDataApplicant>());
			//Console.WriteLine(CodeToSql.GetCreateSp<CallCreditSearchData>());
           
		  //  CreateNewLoanTablesSps();

			CreateInvestorTableSps();
		}

		private static void CreateInvestorTableSps() {
			CodeToFile.Folder = @"c:\temp\";
			CodeToFile.SaveFile<I_FundingType>();
			CodeToFile.SaveFile<I_Grade>();
			CodeToFile.SaveFile<I_GradeRange>();
			CodeToFile.SaveFile<I_Index>();
			CodeToFile.SaveFile<I_Instrument>();
			CodeToFile.SaveFile<I_InterestVariable>();
			CodeToFile.SaveFile<I_Investor>();
			CodeToFile.SaveFile<I_InvestorAccountType>();
			CodeToFile.SaveFile<I_InvestorBankAccount>();
			CodeToFile.SaveFile<I_InvestorBankAccountTransaction>();
			CodeToFile.SaveFile<I_InvestorConfigurationParam>();
			CodeToFile.SaveFile<I_InvestorContact>();
			CodeToFile.SaveFile<I_InvestorOverallStatistics>();
			CodeToFile.SaveFile<I_InvestorSystemBalance>();
			CodeToFile.SaveFile<I_InvestorType>();
			CodeToFile.SaveFile<I_Parameter>();
			CodeToFile.SaveFile<I_Portfolio>();
			CodeToFile.SaveFile<I_Product>();
			CodeToFile.SaveFile<I_ProductSubType>();
			CodeToFile.SaveFile<I_ProductType>();
			CodeToFile.SaveFile<I_UWInvestorConfigurationParam>();
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
            CodeToFile.SaveFile<NL_DecisionRejectReasons>();
            CodeToFile.SaveFile<NL_LoanOptions>();
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


