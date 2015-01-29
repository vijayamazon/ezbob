namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using Exceptions;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;

	public class BrokerLogin : AStrategy {

		public BrokerLogin(string sEmail, Password oPassword) {
			m_oSp = new SpBrokerLogin(DB, Log) {
				Email = sEmail,
				Password = oPassword.Primary,
			};

			Properties = new BrokerProperties();
		} // constructor

		public override string Name { get { return "Broker login"; } } // Name

		public BrokerProperties Properties { get; private set; }

		public override void Execute() {
			m_oSp.FillFirst(Properties);

			if (!string.IsNullOrWhiteSpace(Properties.ErrorMsg))
				throw new StrategyWarning(this, Properties.ErrorMsg);

			SafeReader sr = DB.GetFirst(
				"LoadActiveLotteries",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UserID", Properties.BrokerID),
				new QueryParameter("Now", DateTime.UtcNow)
			);

			Properties.LotteryPlayerID = sr.IsEmpty ? string.Empty : ((Guid)sr["UniqueID"]).ToString("N");
			Properties.LotteryCode = sr["LotteryCode"];
		} // Execute

		private readonly SpBrokerLogin m_oSp;

		private class SpBrokerLogin : AStoredProc {

			public SpBrokerLogin(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				Email = MiscUtils.ValidateStringArg(Email, "Email");
				Password = MiscUtils.ValidateStringArg(m_sPassword, "Password");

				return true;
			} // HasValidParameters

			[UsedImplicitly]
			public string Email { get; set; }

			public string Password {
				[UsedImplicitly]
				get { return SecurityUtils.HashPassword(Email, m_sPassword); }
				set { m_sPassword = value; }
			} // Password

			private string m_sPassword;

		} // class SpBrokerLogin

	} // class BrokerLogin

} // namespace Ezbob.Backend.Strategies.Broker
