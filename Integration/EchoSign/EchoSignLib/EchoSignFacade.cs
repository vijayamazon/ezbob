namespace EchoSignLib {
	using System;
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
		} // constructor

		#endregion constructor

		#endregion public

		#region private

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		private string m_sApiKey;

		#endregion private
	} // class EchoSignFacade
} // namespace
