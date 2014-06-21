namespace EchoSignLib {
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class SpSaveSignedDocument : AStoredProc {
		#region constructor

		public SpSaveSignedDocument(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
		} // constructor

		#endregion constructor

		public override bool HasValidParameters() {
			return
				(EsignatureID > 0) &&
				!string.IsNullOrWhiteSpace(MimeType) &&
				(DocumentContent != null) &&
				(DocumentContent.Length > 0);
		} // HasValidParameters

		public int EsignatureID { get; set; }

		public string MimeType { get; set; }

		public byte[] DocumentContent { get; set; }
	} // class SpSaveSignedDocument
} // namespace
