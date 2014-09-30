namespace EzBob.Backend.Strategies.UserManagement {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class AddCciHistory : AStrategy {
		#region public

		#region constructor

		public AddCciHistory(
			int nCustomerID,
			int nUnderwriterID,
			bool bCciMark,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_bCciMark = bCciMark;
			m_nCustomerID = nCustomerID;
			m_nUnderwriterID = nUnderwriterID;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "AddCciHistory"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			DB.ExecuteNonQuery(
				"AddCciHistory",
				CommandSpecies.StoredProcedure, 
				new QueryParameter("CustomerID", m_nCustomerID),
				new QueryParameter("UnderwriterID", m_nUnderwriterID),
				new QueryParameter("CciMark", m_bCciMark),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nCustomerID;
		private readonly int m_nUnderwriterID;
		private readonly bool m_bCciMark;

		#endregion private
	} // class AddCciHistory
} // namespace
