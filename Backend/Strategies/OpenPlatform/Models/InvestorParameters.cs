namespace Ezbob.Backend.Strategies.OpenPlatform.Models {
    public class InvestorParameters {
        public int InvestorID { get; set; }
        public decimal Balance { get; set; }
        public decimal DailyAvailableAmount { get; set; }
        public decimal WeeklyAvailableAmount { get; set; }

	    /// <summary>
	    /// Returns a string that represents the current object.
	    /// </summary>
	    /// <returns>
	    /// A string that represents the current object.
	    /// </returns>
	    public override string ToString() {
			return string.Format(
@"
InvestorID {0}
Balance {1}
DailyAvailableAmount {2}
WeeklyAvailableAmount {3}", 
				InvestorID, 
				Balance, 
				DailyAvailableAmount, 
				WeeklyAvailableAmount);	
	    }
    }//class InvestorParameters

}//ns
