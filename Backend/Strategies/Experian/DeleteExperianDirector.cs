namespace EzBob.Backend.Strategies.Experian {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class DeleteExperianDirector : AStrategy {
		#region public

		#region constructor

		public DeleteExperianDirector(int nDirectorID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nDirectorID = nDirectorID;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "DeleteExperianDirector"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			DB.ExecuteNonQuery("DeleteExperianDirector", CommandSpecies.StoredProcedure, new QueryParameter("DirectorID", m_nDirectorID));
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nDirectorID;

		#endregion private
	} // class DeleteExperianDirector
} // namespace
