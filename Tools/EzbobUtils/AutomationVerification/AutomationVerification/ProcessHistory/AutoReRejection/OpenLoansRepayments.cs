namespace AutomationCalculator.ProcessHistory.AutoReRejection
{
	public class OpenLoansRepayments : ATrace {
		public OpenLoansRepayments(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { } // constructor

		public void Init(decimal openLoansAmount, decimal repaymentsAmount, decimal portion) {
			if (openLoansAmount == 0) {
				Comment = "customer don't have open loans";
			}
			else {
				Comment = string.Format("Customer re-payed {0} out of {1} in open loans which is {2}% shoud be {3}%",
				                        repaymentsAmount, openLoansAmount, (repaymentsAmount/openLoansAmount*100).ToString("N2"), portion*100);
			}
		}
	}  // class 
} // namespace
