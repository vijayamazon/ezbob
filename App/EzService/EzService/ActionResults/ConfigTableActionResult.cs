namespace EzService {
	using System.Runtime.Serialization;
	using EzBob.Backend.Models;

	[DataContract]
	public class ConfigTableActionResult : ActionResult {
		[DataMember]
		public ConfigTable[] Table { get; set; }
	} // class ConfigTableActionResult
} // namespace
