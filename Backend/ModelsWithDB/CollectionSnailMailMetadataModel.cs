namespace Ezbob.Backend.ModelsWithDB {
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	[DataContract]
	public class CollectionSnailMailMetadataModel {
		[PK]
		[NonTraversable]
		[DataMember]
		public int CollectionSnailMailMetadataID { get; set; }

		[Length(255)]
		[DataMember]
		public string Name { get; set; }

		[Length(255)]
		[DataMember]
		public string ContentType { get; set; }

		[Length(500)]
		[DataMember]
		public string Path { get; set; }

		[NonTraversable]
		[DataMember]
		public byte[] Content { get; set; }
	}
}
