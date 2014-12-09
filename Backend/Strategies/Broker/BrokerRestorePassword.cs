namespace Ezbob.Backend.Strategies.Broker {
	using Exceptions;
	using Ezbob.Backend.Models;
	using MailStrategies;
	using Misc;

	public class BrokerRestorePassword : AStrategy {

		public BrokerRestorePassword(string sMobile, string sCode) {
			m_sMobile = sMobile;
			m_sCode = sCode;
		} // constructor

		public override string Name { get { return "Broker restore password"; } } // Name

		public override void Execute() {
			var oValidator = new ValidateMobileCode(m_sMobile, m_sCode);
			oValidator.Execute();
			if (!oValidator.IsValidatedSuccessfully())
				throw new StrategyWarning(this, "Failed to validate mobile code.");

			var sp = new SpBrokerLoadOwnProperties(DB, Log) {
				ContactMobile = m_sMobile,
			};

			BrokerProperties oProperties = sp.FillFirst<BrokerProperties>();

			if (oProperties == null || oProperties.BrokerID == 0) {
				throw new StrategyWarning(this, string.Format("No broker found with this phone number {0}.", m_sMobile));
			}

			Log.Debug("Broker properties search result for mobile phone {0}:\n{1}", m_sMobile, oProperties);

			new BrokerPasswordRestored(oProperties.BrokerID).Execute();
		} // Execute

		private readonly string m_sMobile;
		private readonly string m_sCode;

	} // class BrokerRestorePassword
} // namespace Ezbob.Backend.Strategies.Broker
