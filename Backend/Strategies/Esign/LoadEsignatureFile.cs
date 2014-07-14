namespace EzBob.Backend.Strategies.Esign {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LoadEsignatureFile : AStrategy {
		#region public

		#region constructor

		public LoadEsignatureFile(long nEsignatureID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nEsignatureID = nEsignatureID;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "LoadEsignatureFile"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			DB.FillFirst(
				this,
				"LoadEsignatureFile",
				CommandSpecies.StoredProcedure,
				new QueryParameter("EsignatureID", m_nEsignatureID)
			);
		} // Execute

		#endregion method Execute

		public string FileName { get; set; }
		public string MimeType { get; set; }
		public byte[] Contents { get; set; }

		#endregion public

		#region private

		private readonly long m_nEsignatureID;

		#endregion private
	} // class LoadEsignatureFile
} // namespace
