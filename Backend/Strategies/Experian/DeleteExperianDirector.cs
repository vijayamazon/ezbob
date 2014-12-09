namespace EzBob.Backend.Strategies.Experian {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class DeleteExperianDirector : AStrategy {

		public DeleteExperianDirector(int nDirectorID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
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
