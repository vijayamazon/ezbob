﻿namespace EzBob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Security;

	public class BrokerUpdatePassword : AStrategy {
		#region public

		#region constructor

		public BrokerUpdatePassword(
			string sContactEmail,
			string sOldPassword,
			string sNewPassword,
			string sNewPassword2,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_oSp = new SpBrokerUpdatePassword(DB, Log) {
				ContactEmail = sContactEmail,
				OldPassword = sOldPassword,
				NewPassword = sNewPassword,
				NewPassword2 = sNewPassword2,
			};

			BrokerID = 0;
		} // constructor

		#endregion constructor

		public override string Name {
			get { return "Broker update password"; }
		} // Name

		public override void Execute() {
			BrokerID = m_oSp.ExecuteScalar<int>();

			if (BrokerID < 1)
				throw new Exception("Failed to update broker password.");
		} // Execute

		public int BrokerID { get; private set; }

		#endregion public

		#region private

		private readonly SpBrokerUpdatePassword m_oSp;

		private class SpBrokerUpdatePassword : AStoredProc {
			public SpBrokerUpdatePassword(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				if (string.IsNullOrWhiteSpace(ContactEmail))
					return false;

				if (string.IsNullOrWhiteSpace(m_sOldPassword))
					return false;

				if (string.IsNullOrWhiteSpace(m_sNewPassword))
					return false;

				if (string.IsNullOrWhiteSpace(NewPassword2))
					return false;

				if (m_sNewPassword != NewPassword2)
					return false;

				if (m_sNewPassword == m_sOldPassword)
					return false;

				return true;
			} // HasValidParameters

			public string ContactEmail { get; set; }

			public string OldPassword {
				get { return SecurityUtils.HashPassword(ContactEmail + m_sOldPassword); }
				set { m_sOldPassword = value; }
			}
			private string m_sOldPassword;

			public string NewPassword {
				get { return SecurityUtils.HashPassword(ContactEmail + m_sNewPassword); }
				set { m_sNewPassword = value; }
			}
			private string m_sNewPassword;

			[NonTraversable]
			public string NewPassword2 { get; set; }
		} // class SpBrokerUpdatePassword

		#endregion private
	} // class BrokerUpdatePassword
} // namespace EzBob.Backend.Strategies.Broker
