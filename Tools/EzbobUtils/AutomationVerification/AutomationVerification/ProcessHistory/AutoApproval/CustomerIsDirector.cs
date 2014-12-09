namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Check if customer is one of companies directors (limited only)
	/// </summary>
	public class CustomerIsDirector : ATrace {
		public CustomerIsDirector(DecisionStatus nDecisionStatus)
			: base(nDecisionStatus)
		{
		} // constructor

		public void Init(bool isLimited) {
			Comment = string.Format("Customer has {0}limited  company", isLimited ? "" : "non-");
		} // Init

		public void Init(string customerName) {
			Comment = string.Format("Customer name is '{0}', no directors received from Experian.", customerName);
		} // Init

		public void Init(string customerName, List<string> directorsNames) {
			Comment = string.Format("Customer name is '{0}' director names:'{1}'", customerName, directorsNames.Aggregate((a,b) => string.Format("{0},{1}", a, b)));
		} // Init
	} // class
} // namespace