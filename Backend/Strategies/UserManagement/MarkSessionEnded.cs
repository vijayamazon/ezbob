namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class MarkSessionEnded : AStrategy {

		public MarkSessionEnded(int nSessionID, string sComment) {
			m_oSp = new SpMarkSessionEnded(nSessionID, sComment, DB, Log);
		} // constructor

		public override string Name {
			get { return "Mark session ended"; }
		} // Name

		public override void Execute() {
			m_oSp.ExecuteNonQuery();
		} // Execute

		private readonly SpMarkSessionEnded m_oSp;

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

	} // class MarkSessionEnded
} // namespace Ezbob.Backend.Strategies.UserManagement
