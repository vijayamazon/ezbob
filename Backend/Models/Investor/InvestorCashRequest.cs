namespace Ezbob.Backend.Models.Investor {
    public class InvestorLoanCashRequest
    {
        public long CashRequestID { get; set; }
        public double ManagerApprovedSum { get; set; }
		public int GradeID { get; set; }
        public decimal FundingType { get; set; }
    }//class InvestorCashRequest

}//ns
