namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;

	internal class CheckUpdateDataRequested : AMainStrategyStep {
		public CheckUpdateDataRequested(
			string outerContextDescription,
			NewCreditLineOption newCreditLineOption,
			int customerID,
			int marketplaceUpdateValidityDays
		) : base(outerContextDescription) {
			this.newCreditLineOption = newCreditLineOption;
			this.customerID = customerID;
			this.marketplaceUpdateValidityDays = marketplaceUpdateValidityDays;

			MarketplacesToUpdate = new List<string>();
		} // constructor

		[StepOutput]
		public List<string> MarketplacesToUpdate { get; private set; }

		public override string Outcome {
			get {
				if (this.newCreditLineOption.UpdateData())
					return "'requested data update'";

				return this.newCreditLineOption.AvoidAutoDecision()
					? "'not requested data update without auto rules'"
					: "'not requested data update with auto rules'";
			} // get
		} // Outcome

		protected override StepResults Run() {
			if (!this.newCreditLineOption.UpdateData()) {
				return this.newCreditLineOption.AvoidAutoDecision()
					? StepResults.NotRequestedWithoutAutoRules
					: StepResults.NotRequestedWithAutoRules;
			} // if

			var now = DateTime.UtcNow;

			DB.ForEachRowSafe(
				sr => {
					DateTime? lastUpdateStart = sr["UpdatingStart"];
					DateTime? lastUpdateEnd = sr["UpdatingEnd"];

					bool shouldUpdate = (lastUpdateStart == null) || ((
						(lastUpdateEnd != null) &&
						(lastUpdateEnd.Value.AddDays(this.marketplaceUpdateValidityDays) <= now)
					));

					if (!shouldUpdate)
						return;

					int mpID = sr["MpID"];

					if (sr["LongUpdateTime"])
						MarketplacesToUpdate.Add(string.Format("{0} marketplace with id {1}", (string)sr["Name"], mpID));
				},
				"LoadMarketplacesLastUpdateTime",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", this.customerID)
			);

			return StepResults.Requested;
		} // Run

		private readonly NewCreditLineOption newCreditLineOption;
		private readonly int customerID;
		private readonly int marketplaceUpdateValidityDays;
	} // class CheckUpdateDataRequested
} // namespace
