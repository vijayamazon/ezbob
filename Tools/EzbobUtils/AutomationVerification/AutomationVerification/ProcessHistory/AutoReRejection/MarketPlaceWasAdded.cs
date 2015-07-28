namespace AutomationCalculator.ProcessHistory.AutoReRejection
{
	public class MarketPlaceWasAdded : ATrace {
		public MarketPlaceWasAdded(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { } // constructor

		public void Init(bool mpWasAdded) {
			Comment = string.Format("Marketplace was{0} added after last decision date", mpWasAdded ? "" : "n't");
		}//Init
	}  // class 
} // namespace
