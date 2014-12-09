﻿namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class SetCustomerPasswordByToken : AStrategy {

		public SetCustomerPasswordByToken(
			string sEmail,
			Password oPassword,
			Guid oToken,
			bool bIsBrokerLead
		) {
			CustomerID = 0;

			m_oData = new UserSecurityData(this) {
				Email = sEmail,
				NewPassword = oPassword.Primary,
			};

			m_oSp = new SpSetCustomerPasswordByToken(DB, Log) {
				Email = sEmail,
				EzPassword = m_oData.NewPasswordHash,
				TokenID = oToken,
				IsBrokerLead = bIsBrokerLead,
			};
		} // constructor

		public override string Name {
			get { return "SetCustomerPasswordByToken"; }
		} // Name

		public override void Execute() {
			Log.Debug("Trying to set a password for token '{0}': {1}...", m_oSp.TokenID, m_oSp.EzPassword);

			m_oData.ValidateEmail();
			m_oData.ValidateNewPassword();

			CustomerID = m_oSp.ExecuteScalar<int>();

			Log.Debug("Setting a password for token '{0}' success: {1}.", m_oSp.TokenID, CustomerID > 0 ? "yes" : "no");
		} // Execute

		public int CustomerID { get; private set; }

		private readonly SpSetCustomerPasswordByToken m_oSp;
		private readonly UserSecurityData m_oData;

		private class SpSetCustomerPasswordByToken : AStoredProc {
			public SpSetCustomerPasswordByToken(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			public override bool HasValidParameters() {
				return
					!string.IsNullOrWhiteSpace(Email) &&
					!string.IsNullOrWhiteSpace(EzPassword) &&
					(TokenID != Guid.Empty);
			} // HasValidParameters

			[UsedImplicitly]
			public string Email { get; set; }

			[UsedImplicitly]
			public string EzPassword { get; set; }

			[UsedImplicitly]
			public Guid TokenID { get; set; }

			[UsedImplicitly]
			public bool IsBrokerLead { get; set; }

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // Now
		} // class SpSetCustomerPasswordByToken

	} // class SetCustomerPasswordByToken
} // namespace
