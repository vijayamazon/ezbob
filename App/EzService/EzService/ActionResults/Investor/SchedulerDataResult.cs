namespace EzService.ActionResults.Investor {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models.Investor;

	[DataContract]
	public class SchedulerDataResult : ActionResult {
		[DataMember]
		public SchedulerDataModel SchedulerData { get; set; }
	}
}


