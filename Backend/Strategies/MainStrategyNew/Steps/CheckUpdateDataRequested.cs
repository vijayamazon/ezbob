namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;

	internal class CheckUpdateDataRequested : AThreeExitStep {
		public CheckUpdateDataRequested(
			string outerContextDescription,
			AMainStrategyStep onRequested,
			AMainStrategyStep onNotRequestedWithAutoRules,
			AMainStrategyStep onNotRequestedWithoutAutoRules,
			NewCreditLineOption newCreditLineOption,
			int customerID,
			int marketplaceUpdateValidityDays
		) : base(outerContextDescription, onRequested, onNotRequestedWithAutoRules, onNotRequestedWithoutAutoRules) {
			this.newCreditLineOption = newCreditLineOption;
			this.customerID = customerID;
			this.marketplaceUpdateValidityDays = marketplaceUpdateValidityDays;

			MarketplacesToUpdate = new List<string>();
		} // constructor

		[StepOutput]
		public List<string> MarketplacesToUpdate { get; private set; }

		protected override string Outcome {
			get {
				if (this.newCreditLineOption.UpdateData())
					return "'requested data update'";

				return this.newCreditLineOption.AvoidAutoDecision()
					? "'not requested data update without auto rules'"
					: "'not requested data update with auto rules'";
			} // get
		} // Outcome

		protected override AMainStrategyStepBase Run() {
			if (!this.newCreditLineOption.UpdateData()) {
				return this.newCreditLineOption.AvoidAutoDecision()
					? OnNotRequestedWithoutAutoRules
					: OnNotRequestedWithAutoRules;
			} // if

			var now = DateTime.UtcNow;

			DB.ForEachRowSafe(
				sr => {
					DateTime lastUpdateTime = sr["UpdatingEnd"];

					if ((now - lastUpdateTime).Days <= this.marketplaceUpdateValidityDays)
						return;

					int mpID = sr["MpID"];

					if (sr["LongUpdateTime"])
						MarketplacesToUpdate.Add(string.Format("{0} marketplace with id {1}", (string)sr["Name"], mpID));
				},
				"LoadMarketplacesLastUpdateTime",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", this.customerID)
			);

			return OnRequested;
		} // Run

		private AMainStrategyStep OnRequested { get { return FirstExit; } }
		private AMainStrategyStep OnNotRequestedWithAutoRules { get { return SecondExit; } }
		private AMainStrategyStep OnNotRequestedWithoutAutoRules { get { return ThirdExit; } }

		private readonly NewCreditLineOption newCreditLineOption;
		private readonly int customerID;
		private readonly int marketplaceUpdateValidityDays;
	} // class CheckUpdateDataRequested
} // namespace
