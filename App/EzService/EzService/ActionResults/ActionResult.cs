namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public abstract class ActionResult {
		protected ActionResult() { }

		protected ActionResult(ActionMetaData metaData) {
			MetaData = metaData;
		}

		[DataMember]
		public ActionMetaData MetaData { get; set; }
	} // struct ActionResult
} // namespace EzService
