namespace Ezbob.Backend.Strategies.MainStrategy {
	using System.Text.RegularExpressions;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using EZBob.DatabaseLib.Model.Database;

	class BackdoorSimpleReject : ABackdoorSimpleDetails {
		public static BackdoorSimpleReject Create(string backdoorCode) {
			var match = regex.Match(backdoorCode);

			if (!match.Success) {
				Log.Debug("Back door code '{0}' ain't no matches rejection regex /{1}/.", backdoorCode, regex);
				return null;
			} // if

			return new BackdoorSimpleReject(
				match.Groups[1].Value == "s" ? CurrentValues.Instance.WizardAutomationTimeout : 0
			);
		} // Create

		public BackdoorSimpleReject(int delay) : base(DecisionActions.Reject, delay) {
		} // constructor

		public override bool SetResult(AutoDecisionResponse response) {
			Log.Debug("Back door simple flow: rejecting..."); 

			response.CreditResult = CreditResultStatus.Rejected;
			response.UserStatus = Status.Rejected;
			response.SystemDecision = SystemDecision.Reject;
			response.DecisionName = "Rejection";
			response.Decision = DecisionActions.Reject;
			response.LoanOfferUnderwriterComment = "Auto rejection - back door simple";

			DoDelay();

			Log.Debug("Back door simple flow: rejected.");

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

		private static readonly Regex regex = new Regex(@"^bds-r([f|s])$");
	} // class BackdoorSimpleReject
} // namespace
