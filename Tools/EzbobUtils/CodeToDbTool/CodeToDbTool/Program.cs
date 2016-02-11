namespace CodeToDbTool {
	using CodeToDbTool.Model;
	using Ezbob.Backend.ModelsWithDB.CompaniesHouse;
	using Ezbob.Utils.dbutils;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;

	class Program {
		static void Main(string[] args) {

			//Console.WriteLine(CodeToSql.GetCreateTable<CallCreditDataApplicant>());
			//Console.WriteLine(CodeToSql.GetCreateSp<CallCreditSearchData>());
           
		  //  CreateNewLoanTablesSps();

			//CreateInvestorTableSps();
			//CreateCompaniesHouseSqls();
			CreateLRSps();
		}

		private static void CreateLRSps() {
			CodeToFile.Folder = @"c:\temp\";
			CodeToFile.SaveSp<LandRegistryDB>();
			CodeToFile.SaveSp<LandRegistryOwnerDB>();
		}

		private static void CreateCompaniesHouseSqls() {
			CodeToFile.Folder = @"c:\temp\";
			CodeToFile.SaveSp<CompaniesHouseOfficerOrder>();
			CodeToFile.SaveSp<CompaniesHouseOfficerOrderItem>();
			CodeToFile.SaveSp<CompaniesHouseOfficerAppointmentOrder>();
			CodeToFile.SaveSp<CompaniesHouseOfficerAppointmentOrderItem>();
			CodeToFile.SaveTable<CompaniesHouseOfficerOrder>();
			CodeToFile.SaveTable<CompaniesHouseOfficerOrderItem>();
			CodeToFile.SaveTable<CompaniesHouseOfficerAppointmentOrder>();
			CodeToFile.SaveTable<CompaniesHouseOfficerAppointmentOrderItem>();
		}


		private static void CreateInvestorTableSps() {
			CodeToFile.Folder = @"c:\temp\";
			CodeToFile.SaveSp<I_FundingType>();
			CodeToFile.SaveSp<I_Grade>();
			CodeToFile.SaveSp<I_GradeRange>();
			CodeToFile.SaveSp<I_Index>();
			CodeToFile.SaveSp<I_Instrument>();
			CodeToFile.SaveSp<I_InterestVariable>();
			CodeToFile.SaveSp<I_Investor>();
			CodeToFile.SaveSp<I_InvestorAccountingConfiguration>();
			CodeToFile.SaveSp<I_InvestorAccountType>();
			CodeToFile.SaveSp<I_InvestorBankAccount>();
			CodeToFile.SaveSp<I_InvestorBankAccountTransaction>();
			CodeToFile.SaveSp<I_InvestorParams>();
			CodeToFile.SaveSp<I_InvestorContact>();
			CodeToFile.SaveSp<I_InvestorOverallStatistics>();
			CodeToFile.SaveSp<I_InvestorSystemBalance>();
			CodeToFile.SaveSp<I_InvestorType>();
			CodeToFile.SaveSp<I_Parameter>();
			CodeToFile.SaveSp<I_Portfolio>();
			CodeToFile.SaveSp<I_Product>();
			CodeToFile.SaveSp<I_ProductSubType>();
			CodeToFile.SaveSp<I_ProductType>();
			CodeToFile.SaveSp<I_SubGrade>();
			CodeToFile.SaveSp<I_OpenPlatformOffer>();
			CodeToFile.SaveSp<I_GradeOriginMap>();
		}

	    private static void CreateNewLoanTablesSps()
	    {
	        CodeToFile.Folder = @"c:\temp\";
            CodeToFile.SaveSp<NL_CashRequests>();
            CodeToFile.SaveSp<NL_Decisions>();
            CodeToFile.SaveSp<NL_Offers>();
            CodeToFile.SaveSp<NL_LoanAgreements>();
            CodeToFile.SaveSp<NL_LoanFeePayments>();
            CodeToFile.SaveSp<NL_BlendedLoans>();
            CodeToFile.SaveSp<NL_FundTransfers>();
            CodeToFile.SaveSp<NL_LoanFees>();
            CodeToFile.SaveSp<NL_LoanHistory>();
            CodeToFile.SaveSp<NL_BlendedOffers>();
            CodeToFile.SaveSp<NL_LoanLegals>();
            CodeToFile.SaveSp<NL_LoanLienLinks>();
            CodeToFile.SaveSp<NL_LoanRollovers>();
            CodeToFile.SaveSp<NL_PaypointTransactions>();
            CodeToFile.SaveSp<NL_PacnetTransactions>();
            CodeToFile.SaveSp<NL_Payments>();
            CodeToFile.SaveSp<NL_LoanSchedulePayments>();
            CodeToFile.SaveSp<NL_LoanSchedules>();
            CodeToFile.SaveSp<NL_LoanStates>();
            CodeToFile.SaveSp<NL_Loans>();
            CodeToFile.SaveSp<NL_DecisionRejectReasons>();
            CodeToFile.SaveSp<NL_LoanOptions>();
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


