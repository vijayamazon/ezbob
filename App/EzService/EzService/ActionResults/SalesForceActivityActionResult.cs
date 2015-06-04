namespace EzService.ActionResults {
	using System.Runtime.Serialization;
	using SalesForceLib.Models;

	[DataContract]
	public class SalesForceActivityActionResult : ActionResult {
		[DataMember]
		public GetActivityResultModel Value { get; set; }
	}
}
