namespace EzService.ActionResults {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models.ApplicationInfo;

	[DataContract]
	public class ApplicationInfoResult : ActionResult {
		[DataMember]
		public ApplicationInfoModel Model { get; set; }
	} // class ApplicationInfoResult
} // namespace
