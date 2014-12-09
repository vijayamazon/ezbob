namespace Ezbob.Backend.Strategies.Broker {
	using Exceptions;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;

	public class BrokerUpdatePassword : AStrategy {

		public BrokerUpdatePassword(
			string sContactEmail,
			Password oOldPassword,
			Password oNewPassword
		) {
			m_oSp = new SpBrokerUpdatePassword(DB, Log) {
				ContactEmail = sContactEmail,
				OldPassword = oOldPassword.Primary,
				NewPassword = oNewPassword.Primary,
				NewPassword2 = oNewPassword.Confirmation,
			};

			BrokerID = 0;
		} // constructor

		public override string Name {
			get { return "Broker update password"; }
		} // Name

		public override void Execute() {
			BrokerID = m_oSp.ExecuteScalar<int>();

			if (BrokerID < 1)
				throw new StrategyWarning(this, "Failed to update broker password.");
		} // Execute

		public int BrokerID { get; private set; }

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

			[UsedImplicitly]
			public string ContactEmail { get; set; }

			[UsedImplicitly]
			public string OldPassword {
				get { return SecurityUtils.HashPassword(ContactEmail, m_sOldPassword); }
				set { m_sOldPassword = value; }
			}
			private string m_sOldPassword;

			[UsedImplicitly]
			public string NewPassword {
				get { return SecurityUtils.HashPassword(ContactEmail, m_sNewPassword); }
				set { m_sNewPassword = value; }
			}
			private string m_sNewPassword;

			[NonTraversable]
			public string NewPassword2 { private get; set; }
		} // class SpBrokerUpdatePassword

	} // class BrokerUpdatePassword
} // namespace Ezbob.Backend.Strategies.Broker
