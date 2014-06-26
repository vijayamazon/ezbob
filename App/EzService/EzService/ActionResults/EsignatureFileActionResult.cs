namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public class EsignatureFileActionResult : ActionResult {
		[DataMember]
		public string FileName { get; set; }

		[DataMember]
		public string MimeType { get; set; }

		[DataMember]
		public byte[] Contents { get; set; }
	} // class EsignatureFileActionResult
} // namespace
