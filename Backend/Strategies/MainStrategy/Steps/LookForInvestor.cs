namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using Ezbob.Backend.Strategies.Investor;

	internal class LookForInvestor : AMainStrategyStep {
		public LookForInvestor(
			string outerContextDescription,
			int customerID,
			long cashRequestID,
			int underwriterID
		) : base(outerContextDescription) {
			this.customerID = customerID;
			this.cashRequestID = cashRequestID;
			this.underwriterID = underwriterID;
		} // constructor

		public override string Outcome { get { return this.outcome; } }

		protected override StepResults Run() {
			// TODO add backdoor logic
			var loti = new LinkOfferToInvestor(this.customerID, this.cashRequestID, false, null, this.underwriterID);
			loti.Execute();

			bool investorFound = !loti.IsForOpenPlatform || loti.FoundInvestor;

			this.outcome = string.Format(
				"'investor {0}found: for Open platform = {1}, really found = {2}'",
				investorFound ? string.Empty : "not ",
				loti.IsForOpenPlatform ? "yes" : "no",
				loti.FoundInvestor ? "yes" : "no"
			);

			return investorFound ? StepResults.Found : StepResults.NotFound;
		} // Run

		private readonly int customerID;
		private readonly long cashRequestID;
		private readonly int underwriterID;

		private string outcome;
	} // class LookForInvestor
} // namespace
