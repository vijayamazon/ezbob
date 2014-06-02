﻿namespace EzBob.Backend.Strategies.Broker {
	using Exceptions;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Security;

	#region class BrokerLogin

	public class BrokerLogin : AStrategy {
		#region public

		#region constructor

		public BrokerLogin(string sEmail, string sPassword, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpBrokerLogin(DB, Log) {
				Email = sEmail,
				Password = sPassword,
			};

			Properties = new BrokerProperties();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Broker login"; } } // Name

		#endregion property Name

		#region property Properties

		public BrokerProperties Properties { get; private set; }

		#endregion property Properties

		#region method Execute

		public override void Execute() {
			m_oSp.FillFirst(Properties);

			if (!string.IsNullOrWhiteSpace(Properties.ErrorMsg))
				throw new StrategyWarning(this, Properties.ErrorMsg);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly SpBrokerLogin m_oSp;

		#region class SpBrokerLogin

		private class SpBrokerLogin : AStoredProc {
			#region constructor

			public SpBrokerLogin(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			#endregion constructor

			#region method HasValidParameters

			public override bool HasValidParameters() {
				Email = MiscUtils.ValidateStringArg(Email, "Email");
				Password = MiscUtils.ValidateStringArg(m_sPassword, "Password");

				return true;
			} // HasValidParameters

			#endregion method HasValidParameters

			#region property Email

			public string Email { get; set; }

			#endregion property Email

			#region property Password

			public string Password {
				get { return SecurityUtils.HashPassword(Email, m_sPassword); }
				set { m_sPassword = value; }
			} // Password

			private string m_sPassword;

			#endregion property Password
		} // class SpBrokerLogin

		#endregion class SpBrokerLogin

		#endregion private
	} // class BrokerLogin

	#endregion class BrokerLogin
} // namespace EzBob.Backend.Strategies.Broker
