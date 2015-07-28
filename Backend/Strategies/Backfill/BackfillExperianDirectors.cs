namespace Ezbob.Backend.Strategies.Experian {
	using System.Collections.Generic;
	using Ezbob.Database;

	public class BackfillExperianDirectors : AStrategy {
		public BackfillExperianDirectors(int? nCustomerID) {
			m_nCustomerID = nCustomerID.HasValue ? (nCustomerID > 0 ? nCustomerID : null) : null;
		} // constructor

		public override string Name {
			get { return "BackfillExperianDirectors"; }
		} // Name

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
				new UpdateLimitedExperianDirectors(sr["CustomerID"], sr["Id"]).Execute();
		} // Execute

		private int? m_nCustomerID;
	} // class BackfillExperianDirectors
} // namespace
