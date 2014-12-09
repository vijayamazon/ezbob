namespace Ezbob.Backend.Strategies.Experian {
	using Ezbob.Database;

	public class DeleteExperianDirector : AStrategy {
		public DeleteExperianDirector(int nDirectorID) {
			m_nDirectorID = nDirectorID;
		} // constructor

		public override string Name {
			get { return "DeleteExperianDirector"; }
		} // Name

		public override void Execute() {
			DB.ExecuteNonQuery("DeleteExperianDirector", CommandSpecies.StoredProcedure, new QueryParameter("DirectorID", m_nDirectorID));
		} // Execute

		private readonly int m_nDirectorID;
	} // class DeleteExperianDirector
} // namespace
