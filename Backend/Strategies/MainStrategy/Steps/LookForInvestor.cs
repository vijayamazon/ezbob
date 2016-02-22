namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using Ezbob.Backend.Strategies.Investor;

	internal class LookForInvestor : AMainStrategyStep {
		public LookForInvestor(
			string outerContextDescription,
			int customerID,
			long cashRequestID,
			int underwriterID,
			bool backdoorLogicApplied,
			int? backdoorInvestorID
		) : base(outerContextDescription) {
			this.customerID = customerID;
			this.cashRequestID = cashRequestID;
			this.underwriterID = underwriterID;
			this.backdoorLogicApplied = backdoorLogicApplied;
			this.backdoorInvestorID = backdoorInvestorID;
		} // constructor

		public override string Outcome { get { return this.outcome; } }

		protected override StepResults Run() {
			LinkOfferToInvestor loti = null;

			if (this.backdoorLogicApplied) {
				if (this.backdoorInvestorID == null)
					Log.Debug("Back door investor for {0}: look for investor as usual.", OuterContextDescription);
				else if (this.backdoorInvestorID <= 0) {
					this.outcome = string.Format("'back door investor not found'");
					return StepResults.NotFound;
				} else {
					Log.Debug(
						"Back door investor for {0}: try to set investor with id {1}.",
						OuterContextDescription,
						this.backdoorInvestorID.Value
					);

					loti = new LinkOfferToInvestor(
						this.customerID,
						this.cashRequestID,
						true,
						this.backdoorInvestorID.Value,
						this.underwriterID
					);
				} // if
			} // if back door logic

			if (loti == null)
				loti = new LinkOfferToInvestor(this.customerID, this.cashRequestID, false, null, this.underwriterID);

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
		private readonly bool backdoorLogicApplied;
		private readonly int? backdoorInvestorID;

		private string outcome;
	} // class LookForInvestor
} // namespace
