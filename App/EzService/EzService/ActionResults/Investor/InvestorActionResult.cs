namespace EzService.ActionResults.Investor {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models.Investor;

	[DataContract]
	public class InvestorActionResult : ActionResult {
		[DataMember]
		public InvestorModel Investor { get; set; }
	}
}
