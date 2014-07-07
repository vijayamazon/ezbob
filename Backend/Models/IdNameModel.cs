namespace Ezbob.Backend.Models {
	using System.Runtime.Serialization;
	using Utils;

	[DataContract]
	public class IdNameModel : ITraversable {
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public string Name { get; set; }
	} // class IdNameModel
} // namespace
