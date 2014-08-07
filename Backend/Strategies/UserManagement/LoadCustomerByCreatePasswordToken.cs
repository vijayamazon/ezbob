namespace EzBob.Backend.Strategies.UserManagement {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class LoadCustomerByCreatePasswordToken : AStrategy {
		#region public

		#region constructor

		public LoadCustomerByCreatePasswordToken(Guid oToken, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpLoadCustomerByCreatePasswordToken(oToken, DB, Log);
			Result = new CustomerDetails();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "LoadCustomerByCreatePasswordToken"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oSp.GetFirst().Fill(Result);
			Log.Debug("Result for token id {0} is: {1}.", m_oSp.TokenID, Result);
		} // Execute

		#endregion method Execute

		#region property Result

		public CustomerDetails Result { get; private set; }

		#endregion property Result

		#endregion public

		#region private

		private readonly SpLoadCustomerByCreatePasswordToken m_oSp;

		#region class SpLoadCustomerByCreatePasswordToken

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

		#endregion class SpLoadCustomerByCreatePasswordToken

		#endregion private
	} // class LoadCustomerByCreatePasswordToken
} // namespace
