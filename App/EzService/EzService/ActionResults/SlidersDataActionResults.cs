namespace EzService.ActionResults {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class SlidersDataActionResults : ActionResult {
		[DataMember]
		public SlidersDataModel SlidersData { get; set; }
	}
}


