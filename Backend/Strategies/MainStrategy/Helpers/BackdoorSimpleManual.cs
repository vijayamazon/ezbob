namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System.Text.RegularExpressions;
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using EZBob.DatabaseLib.Model.Database;

	class BackdoorSimpleManual : ABackdoorSimpleDetails {
		public static ABackdoorSimpleDetails Create(
			string backdoorCode,
			int customerID,
			bool ownsProperty,
			decimal requestedAmount,
			int homeOwnerCap,
			int notHomeOwnerCap,
			int delay
		) {
			var match = regex.Match(backdoorCode);

			if (!match.Success) {
				Log.Debug("Back door code '{0}' ain't no matches manual regex /{1}/.", backdoorCode, regex);
				return null;
			} // if

			return new BackdoorSimpleManual(
				homeOwnerCap,
				notHomeOwnerCap,
				customerID,
				match.Groups[1].Value == "s" ? delay : 0,
				ownsProperty,
				requestedAmount
			);
		} // Create

		public override bool SetResult(AutoDecisionResponse response) {
			Log.Debug("Back door simple flow: using manual decision...");

			if (!CalculateMedalAndOffer()) {
				Log.Debug(
					"Back door simple flow: failed to set manual for customer {0} because of medal/offer.",
					this.customerID
				);
				return false;
			} // if

			response.CreditResult = CreditResultStatus.WaitingForDecision;
			response.UserStatus = Status.Manual;
			response.SystemDecision = SystemDecision.Manual;

			response.HasApprovalChance = false;

			DoDelay();

			Log.Debug("Back door simple flow: manual decision.");

			return true;
		} // SetResults

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format("back door decision '{0}' after '{1}' seconds.", Decision, Delay);
		} // ToString

		private BackdoorSimpleManual(
			int homeOwnerCap,
			int notHomeOwnerCap,
			int customerID,
			int delay,
			bool ownsProperty,
			decimal requestedAmount
		) : base(homeOwnerCap, notHomeOwnerCap, customerID, DecisionActions.Waiting, delay, ownsProperty, requestedAmount) {
		} // constructor

		private static readonly Regex regex = new Regex(@"^bds-m([fs])$");
	} // class BackdoorSimpleManual
} // namespace
