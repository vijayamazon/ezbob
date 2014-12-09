namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using Ezbob.Database;

	public class AddCciHistory : AStrategy {

		public AddCciHistory(
			int nCustomerID,
			int nUnderwriterID,
			bool bCciMark
		) {
			m_bCciMark = bCciMark;
			m_nCustomerID = nCustomerID;
			m_nUnderwriterID = nUnderwriterID;
		} // constructor

		public override string Name {
			get { return "AddCciHistory"; }
		} // Name

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

		private readonly int m_nCustomerID;
		private readonly int m_nUnderwriterID;
		private readonly bool m_bCciMark;

	} // class AddCciHistory
} // namespace
