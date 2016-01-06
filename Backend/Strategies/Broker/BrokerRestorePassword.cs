namespace Ezbob.Backend.Strategies.Broker {
	using Exceptions;
	using Ezbob.Backend.Models;
	using MailStrategies;
	using Misc;

	public class BrokerRestorePassword : AStrategy {
		public BrokerRestorePassword(string sMobile, string sCode) {
			this.mobilePhoneNumber = sMobile;
			this.confirmationCode = sCode;
		} // constructor

		public override string Name { get { return "Broker restore password"; } } // Name

		public override void Execute() {
			var oValidator = new ValidateMobileCode(this.mobilePhoneNumber, this.confirmationCode);
			oValidator.Execute();
			if (!oValidator.IsValidatedSuccessfully())
				throw new StrategyWarning(this, "Failed to validate mobile code.");

			var sp = new SpBrokerLoadOwnProperties(DB, Log) {
				ContactMobile = this.mobilePhoneNumber,
			};

			BrokerProperties props = sp.FillFirst<BrokerProperties>();

			if ((props == null) || (props.BrokerID == 0)) {
				throw new StrategyWarning(this, string.Format(
					"No broker found with this phone number {0}.",
					this.mobilePhoneNumber
				));
			} // if

			Log.Debug("Broker properties search result for mobile phone {0}:\n{1}", this.mobilePhoneNumber, props);

			new BrokerPasswordRestored(props.BrokerID).Execute();
		} // Execute

		private readonly string mobilePhoneNumber;
		private readonly string confirmationCode;
	} // class BrokerRestorePassword
} // namespace Ezbob.Backend.Strategies.Broker
