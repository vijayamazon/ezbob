namespace Ezbob.Backend.Models.Investor {
    public class InvestorLoanCashRequest
    {
        public long CashRequestID { get; set; }
        public decimal ManagerApprovedSum { get; set; }
		public int GradeID { get; set; }
        public decimal FundingType { get; set; }

	    /// <summary>
	    /// Returns a string that represents the current object.
	    /// </summary>
	    /// <returns>
	    /// A string that represents the current object.
	    /// </returns>
	    public override string ToString() {
			return string.Format(
@"CashRequestID: {0}
ManagerApprovedSum: {1}
GradeID: {2}
FundingType: {3}", CashRequestID, ManagerApprovedSum, GradeID, FundingType);
	    }
    }//class InvestorCashRequest

}//ns
