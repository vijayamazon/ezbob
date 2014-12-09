namespace EzBob.Backend.Strategies.Esign {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LoadEsignatureFile : AStrategy {

		public LoadEsignatureFile(long nEsignatureID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nEsignatureID = nEsignatureID;
		} // constructor

		public override string Name {
			get { return "LoadEsignatureFile"; }
		} // Name

		public override void Execute() {
			DB.FillFirst(
				this,
				"LoadEsignatureFile",
				CommandSpecies.StoredProcedure,
				new QueryParameter("EsignatureID", m_nEsignatureID)
			);
		} // Execute

		public string FileName { get; set; }
		public string MimeType { get; set; }
		public byte[] Contents { get; set; }

		private readonly long m_nEsignatureID;

	} // class LoadEsignatureFile
} // namespace
