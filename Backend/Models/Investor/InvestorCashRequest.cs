namespace Ezbob.Backend.Models.Investor {
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;

    public class InvestorCashRequest
    {
        public double ManagerApprovedSum { get; set; }
		public double ManagerApprovedSumFinal { get; set; }
		public Grade Grade { get; set; }
    }//class InvestorCashRequest

}//ns
