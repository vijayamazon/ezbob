namespace Ezbob.Backend.ModelsWithDB.Investor {
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;

    public class InvestorLoanCashRequest
    {
        public long CashRequestID { get; set; }
        public decimal ManagerApprovedSum { get; set; }
		public int GradeID { get; set; }
        public decimal FundingType { get; set; }
    }//class InvestorCashRequest

}//ns
