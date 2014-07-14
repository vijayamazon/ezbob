namespace Ezbob.Backend.Models {
	using System.Runtime.Serialization;

	[DataContract]
	public class IdNameModel {
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public string Name { get; set; }
	} // class IdNameModel
} // namespace
