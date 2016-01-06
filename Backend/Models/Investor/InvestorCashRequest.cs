namespace Ezbob.Backend.Models.Investor {
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;

    public class InvestorCashRequest
    {
        public long CashRequestID { get; set; }
        public double ManagerApprovedSum { get; set; }
		public Grade Grade { get; set; }
    }//class InvestorCashRequest

}//ns
