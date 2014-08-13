namespace EzBob.Backend.Strategies.UserManagement.EmailConfirmation {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class EmailConfirmationLoad : AStrategy {
		#region public

		#region constructor

		public EmailConfirmationLoad(int nUserID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			ErrorMessage = "email confirmation state is not yet loaded";
			m_nUserID = nUserID;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "EmailConfirmationLoad"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			SafeReader sr = DB.GetFirst(
				"LoadEmailConfirmationState",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UserID", m_nUserID)
			);

			if ((int)sr["UserID"] != m_nUserID)
				ErrorMessage = "failed to load email confirmation state";
			else if (!sr["IsConfirmed"])
				ErrorMessage = "email is not confirmed";
			else
				ErrorMessage = null;
		} // Execute

		#endregion method Execute

		#region property IsConfirmed

		public bool IsConfirmed {
			get { return string.IsNullOrWhiteSpace(ErrorMessage); }
		} // IsConfirmed

		#endregion property IsConfirmed

		#region property ErrorMessage

		public string ErrorMessage { get; private set; }

		#endregion property ErrorMessage

		#endregion public

		#region private

		private readonly int m_nUserID;

		#endregion private
	} // class EmailConfirmationLoad
} // namespace
