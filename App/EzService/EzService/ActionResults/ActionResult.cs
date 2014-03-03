namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public abstract class ActionResult {
		[DataMember]
		public ActionMetaData MetaData { get; set; }
	} // struct ActionResult
} // namespace EzService
