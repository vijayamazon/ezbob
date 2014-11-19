namespace AutomationCalculator.ProcessHistory.AutoReRejection
{
	public class MarketPlaceWasAdded : ABoolTrace {
		public MarketPlaceWasAdded(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { } // constructor

		protected override string PropertyName {
			get { return "Market place was added after last manual reject"; }
		} // PropertyName
	}  // class 
} // namespace
