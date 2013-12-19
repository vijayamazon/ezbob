using System.Runtime.Serialization;

namespace EzService {
	#region class ActionResult

	[DataContract]
	public abstract class ActionResult {
		[DataMember]
		public ActionMetaData MetaData { get; set; }
	} // struct ActionResult

	#endregion class ActionResult
} // namespace EzService
