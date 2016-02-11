namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System.Text.RegularExpressions;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using EZBob.DatabaseLib.Model.Database;

	class BackdoorSimpleManual : ABackdoorSimpleDetails {
		public static BackdoorSimpleManual Create(string backdoorCode, int customerID) {
			var match = regex.Match(backdoorCode);

			if (!match.Success) {
				Log.Debug("Back door code '{0}' ain't no matches manual regex /{1}/.", backdoorCode, regex);
				return null;
			} // if

			return new BackdoorSimpleManual(
				customerID,
				match.Groups[1].Value == "s" ? CurrentValues.Instance.WizardAutomationTimeout : 0
			);
		} // Create

		public override bool SetResult(AutoDecisionResponse response) {
			Log.Debug("Back door simple flow: using manual decision...");

			CalculateMedalAndOffer(null, 0);

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

		private BackdoorSimpleManual(int customerID, int delay) : base(customerID, DecisionActions.Waiting, delay) {
		} // constructor

		private static readonly Regex regex = new Regex(@"^bds-m([fs])$");
	} // class BackdoorSimpleManual
} // namespace
