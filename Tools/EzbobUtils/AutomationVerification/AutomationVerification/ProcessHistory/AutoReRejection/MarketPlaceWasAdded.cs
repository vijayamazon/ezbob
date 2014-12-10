namespace AutomationCalculator.ProcessHistory.AutoReRejection
{
	public class MarketPlaceWasAdded : ABoolTrace {
		public MarketPlaceWasAdded(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { } // constructor

		protected override string PropertyName {
			get { return "Marketplace was added after last decision date"; }
		} // PropertyName
	}  // class 
} // namespace
