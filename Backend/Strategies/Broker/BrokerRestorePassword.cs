namespace EzBob.Backend.Strategies.Broker {
	using Exceptions;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailStrategies;
	using Misc;

	public class BrokerRestorePassword : AStrategy {
		#region public

		#region constructor

		public BrokerRestorePassword(string sMobile, string sCode, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_sMobile = sMobile;
			m_sCode = sCode;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Broker restore password"; } } // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			var oValidator = new ValidateMobileCode(m_sMobile, m_sCode, DB, Log);
			oValidator.Execute();
			if (!oValidator.IsValidatedSuccessfully())
				throw new StrategyWarning(this, "Failed to validate mobile code.");

			var sp = new SpBrokerLoadOwnProperties(DB, Log) {
				ContactMobile = m_sMobile,
			};

			BrokerProperties oProperties = sp.FillFirst<BrokerProperties>();

			Log.Debug("Broker properties search result for mobile phone {0}:\n{1}", m_sMobile, oProperties);

			new BrokerPasswordRestored(oProperties.BrokerID, DB, Log).Execute();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly string m_sMobile;
		private readonly string m_sCode;

		#endregion private
	} // class BrokerRestorePassword
} // namespace EzBob.Backend.Strategies.Broker
