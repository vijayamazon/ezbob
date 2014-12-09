namespace Ezbob.Backend.Strategies.Esign {
	using Ezbob.Database;

	public class LoadEsignatureFile : AStrategy {

		public LoadEsignatureFile(long nEsignatureID) {
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
