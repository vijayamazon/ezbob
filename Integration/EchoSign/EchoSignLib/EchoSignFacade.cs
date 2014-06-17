namespace EchoSignLib {
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Exceptions;

	public class EchoSignFacade {
		#region public

		#region constructor

		public EchoSignFacade(AConnection oDB, ASafeLog oLog) {
			m_oLog = oLog ?? new SafeLog();

			if (oDB == null)
				throw new Alert(m_oLog, "Cannot create EchoSign façade: database connection not specified.");

			m_oDB = oDB;

			LoadConfiguration();

			m_oLog.Msg("EchoSign façade is ready.");
		} // constructor

		#endregion constructor

		#endregion public

		#region private

		#region method LoadConfiguration

		private void LoadConfiguration() {
			m_sApiKey = CurrentValues.Instance.EchoSignApiKey;

			m_oLog.Debug("************************************************************************");
			m_oLog.Debug("*");
			m_oLog.Debug("* EchoSign façade configuration - begin:");
			m_oLog.Debug("*");
			m_oLog.Debug("************************************************************************");

			m_oLog.Debug("API key: {0}", m_sApiKey);

			m_oLog.Debug("************************************************************************");
			m_oLog.Debug("*");
			m_oLog.Debug("* EchoSign façade configuration - end.");
			m_oLog.Debug("*");
			m_oLog.Debug("************************************************************************");
		} // LoadConfiguration

		#endregion method LoadConfiguration

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		private string m_sApiKey;

		#endregion private
	} // class EchoSignFacade
} // namespace
