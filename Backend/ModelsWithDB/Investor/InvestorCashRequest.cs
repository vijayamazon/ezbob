namespace Ezbob.Backend.ModelsWithDB.Investor {
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;

    public class InvestorLoanCashRequest
    {
        public long CashRequestID { get; set; }
        public double ManagerApprovedSum { get; set; }
		public Grade Grade { get; set; }
        public decimal FundingType { get; set; }
    }//class InvestorCashRequest

}//ns
