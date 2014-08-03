namespace EzBob.Backend.Strategies.Experian {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BackfillExperianDirectors : AStrategy {
		#region constructor

		public BackfillExperianDirectors(int? nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID.HasValue ? (nCustomerID > 0 ? nCustomerID : null) : null;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "BackfillExperianDirectors"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			if (m_nCustomerID.HasValue)
				DB.ExecuteNonQuery("DELETE FROM ExperianDirectors WHERE CustomerID = " + m_nCustomerID.Value, CommandSpecies.Text);
			else
				DB.ExecuteNonQuery("DELETE FROM ExperianDirectors", CommandSpecies.Text);

			IEnumerable<SafeReader> lst = DB.ExecuteEnumerable(
				"LoadExperianDataForDirectorsBackfill",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", m_nCustomerID)
			);

			foreach (SafeReader sr in lst)
				new UpdateLimitedExperianDirectors(sr["CustomerID"], sr["Id"], DB, Log).Execute();
		} // Execute

		#endregion method Execute

		private int? m_nCustomerID;
	} // class BackfillExperianDirectors
} // namespace
