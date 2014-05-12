namespace Ezbob.Backend.Models {
	using System.Runtime.Serialization;
	using Utils;

	[DataContract]
	public class FileDescription : ITraversable {
		[DataMember]
		public string ID { get; set; }

		[DataMember]
		public string FileID { get; set; }

		[DataMember]
		public string FileName { get; set; }

		[DataMember]
		public string DisplayName { get; set; }

		[DataMember]
		public string MimeType { get; set; }

		[DataMember]
		public int SortPosition { get; set; }
	} // class FileDescription
} // namespace Ezbob.Backend.Models
