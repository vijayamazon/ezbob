namespace EchoSignLib {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class EchoSignFacade {
		#region public

		#region constructor

		public EchoSignFacade(AConnection oDB, ASafeLog oLog) {
			m_oDB = oDB;
			m_oLog = oLog ?? new SafeLog();
		} // constructor

		#endregion constructor

		#endregion public

		#region protected
		#endregion protected

		#region private

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		#endregion private
	} // class EchoSignFacade
} // namespace
