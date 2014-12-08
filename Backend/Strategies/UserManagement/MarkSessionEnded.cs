namespace EzBob.Backend.Strategies.UserManagement {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class MarkSessionEnded : AStrategy {
		#region public

		#region constructor

		public MarkSessionEnded(int nSessionID, string sComment, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpMarkSessionEnded(nSessionID, sComment, DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Mark session ended"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oSp.ExecuteNonQuery();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly SpMarkSessionEnded m_oSp;

		#region class SpMarkSessionEnded

		private class SpMarkSessionEnded : AStoredProc {
			public SpMarkSessionEnded(int nSessionID, string sComment, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				SessionID = nSessionID;
				Comment = sComment;
			} // constructor

			public override bool HasValidParameters() {
				return SessionID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public int SessionID { get; set; }

			[UsedImplicitly]
			public string Comment { get; set; }

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				set { }
			} // Now
		} // class SpMarkSessionEnded

		#endregion class SpMarkSessionEnded

		#endregion private
	} // class MarkSessionEnded
} // namespace EzBob.Backend.Strategies.UserManagement
