namespace EzBob.Backend.Strategies.Broker {
	using System;
	using Exceptions;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;

	#region class BrokerLogin

	public class BrokerLogin : AStrategy {
		public BrokerLogin(string sEmail, Password oPassword, string promotionName, DateTime? promotionPageVisitTime, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpBrokerLogin(DB, Log) {
				Email = sEmail,
				Password = oPassword.Primary,
				LotteryCode = promotionName,
				PageVisitTime = promotionPageVisitTime,
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

			SafeReader sr = DB.GetFirst(
				"LoadActiveLotteries",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UserID", Properties.BrokerID),
				new QueryParameter("Now", DateTime.UtcNow)
			);

			Properties.LotteryPlayerID = sr.IsEmpty ? string.Empty : ((Guid)sr["UniqueID"]).ToString("N");
			Properties.LotteryCode = sr["LotteryCode"];
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

			[UsedImplicitly]
			public string Email { get; set; }

			#endregion property Email

			#region property Password

			public string Password {
				[UsedImplicitly]
				get { return SecurityUtils.HashPassword(Email, m_sPassword); }
				set { m_sPassword = value; }
			} // Password

			[UsedImplicitly]
			public string LotteryCode { get; set; }

			[UsedImplicitly]
			public DateTime? PageVisitTime { get; set; }

			private string m_sPassword;
		} // class SpBrokerLogin

		#endregion class SpBrokerLogin

		#endregion private
	} // class BrokerLogin

	#endregion class BrokerLogin
} // namespace EzBob.Backend.Strategies.Broker
