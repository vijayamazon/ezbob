namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class FileListActionResult : ActionResult {
		[DataMember]
		public FileDescription[] Files { get; set; }
	} // class FileListActionResult
} // namespace EzService
