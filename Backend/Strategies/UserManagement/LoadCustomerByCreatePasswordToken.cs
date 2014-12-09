namespace EzBob.Backend.Strategies.UserManagement {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class LoadCustomerByCreatePasswordToken : AStrategy {

		public LoadCustomerByCreatePasswordToken(Guid oToken, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpLoadCustomerByCreatePasswordToken(oToken, DB, Log);
			Result = new CustomerDetails();
		} // constructor

		public override string Name {
			get { return "LoadCustomerByCreatePasswordToken"; }
		} // Name

		public override void Execute() {
			m_oSp.GetFirst().Fill(Result);
			Log.Debug("Result for token id {0} is: {1}.", m_oSp.TokenID, Result);
		} // Execute

		public CustomerDetails Result { get; private set; }

		private readonly SpLoadCustomerByCreatePasswordToken m_oSp;

		private class SpLoadCustomerByCreatePasswordToken : AStoredProc {
			public SpLoadCustomerByCreatePasswordToken(Guid oTokenID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				TokenID = oTokenID;
			} // constructor

			public override bool HasValidParameters() {
				return TokenID != Guid.Empty;
			} // HasValidParameters

			[UsedImplicitly]
			public Guid TokenID { get; set; }

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // Now
		} // class SpLoadCustomerByCreatePasswordToken

	} // class LoadCustomerByCreatePasswordToken
} // namespace
