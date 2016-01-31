namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System.Threading;
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;

	internal abstract class ABackdoorSimpleDetails {
		public static ABackdoorSimpleDetails Create(CustomerDetails customer) {
			if (customer == null) {
				Log.Alert("Cannot check back door simple usage: customer is null.");

				return null;
			} // if

			return Create(customer.ID, customer.AppEmail, customer.OwnsProperty);
		} // Create

		public static ABackdoorSimpleDetails Create(int customerID, string email, bool ownsProperty) {
			if (string.IsNullOrWhiteSpace(email)) {
				Log.Alert(
					"Not using back door simple for customer '{0}': customer email not specified.",
					customerID
				);

				return null;
			} // if

			int atPos = email.LastIndexOf('@');

			if (atPos < 0) {
				Log.Alert(
					"Not using back door simple for customer '{0}': customer email '{1}' contains no '@'.",
					customerID,
					email
				);

				return null;
			} // if

			int plusPos = email.LastIndexOf('+', atPos - 1);

			if (plusPos < 0) {
				Log.Debug(
					"Not using back door simple for customer '{0}': customer email '{1}' contains no '+'.",
					customerID,
					email
				);

				return null;
			} // if

			string backdoorCode = email.Substring(plusPos + 1, atPos - plusPos - 1);

			ABackdoorSimpleDetails result =
				(ABackdoorSimpleDetails)BackdoorSimpleReject.Create(backdoorCode) ??
				(ABackdoorSimpleDetails)BackdoorSimpleApprove.Create(backdoorCode, customerID, ownsProperty) ??
				(ABackdoorSimpleDetails)BackdoorSimpleManual.Create(backdoorCode);

			if (result != null) {
				Log.Debug("Using back door simple for customer '{0}' as: {1}.", customerID, result);
				return result;
			} // if

			Log.Debug(
				"Not using back door simple for customer '{0}': back door code '{1}' from customer email '{2}' " +
				"ain't no matches any existing back door regex.",
				customerID,
				backdoorCode,
				email
			);

			return null;
		} // Create

		public DecisionActions Decision { get; private set; }

		public int Delay { get; private set; }

		public abstract bool SetResult(AutoDecisionResponse response);

		protected ABackdoorSimpleDetails(DecisionActions decision, int delay) {
			Decision = decision;
			Delay = delay;
		} // constructor

		protected static ASafeLog Log {
			get { return Library.Instance.Log; }
		} // Log

		protected virtual void DoDelay() {
			if (Delay < 1)
				return;

			//fix for too small delay configuration
			if (Delay < 20) {
				Delay = 20;
			}
			Log.Debug(
				"Back door simple flow: delaying for {0}...",
				Grammar.Number(Delay, "second")
			);

			for (int i = 0; i < Delay; i++) {
				Thread.Sleep(1000);

				Log.Debug(
					"Back door simple flow: {0} of {1} delay have passed.",
					Grammar.Number(i + 1, "second"),
					Grammar.Number(Delay, "second")
				);
			} // for each delay second

			Log.Debug(
				"Back door simple flow: delaying for {0} complete.",
				Grammar.Number(Delay, "second")
			);
		} // DoDelay
	} // class ABackdoorSimpleDetails
} // namespace
