namespace EzBob.Backend.Strategies.UserManagement {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class SetCustomerPasswordByToken : AStrategy {
		#region public

		#region constructor

		public SetCustomerPasswordByToken(
			string sEmail,
			Password oPassword,
			Guid oToken,
			bool bIsBrokerLead,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
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

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "SetCustomerPasswordByToken"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Log.Debug("Trying to set a password for token '{0}': {1}...", m_oSp.TokenID, m_oSp.EzPassword);

			m_oData.ValidateEmail();
			m_oData.ValidateNewPassword();

			CustomerID = m_oSp.ExecuteScalar<int>();

			Log.Debug("Setting a password for token '{0}' success: {1}.", m_oSp.TokenID, CustomerID > 0 ? "yes" : "no");
		} // Execute

		#endregion method Execute

		#region property CustomerID

		public int CustomerID { get; private set; }

		#endregion property CustomerID

		#endregion public

		#region private

		private readonly SpSetCustomerPasswordByToken m_oSp;
		private readonly UserSecurityData m_oData;

		#region class SpSetCustomerPasswordByToken

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

		#endregion class SpSetCustomerPasswordByToken

		#endregion private
	} // class SetCustomerPasswordByToken
} // namespace
