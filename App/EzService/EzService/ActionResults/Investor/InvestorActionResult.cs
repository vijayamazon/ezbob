namespace EzService.ActionResults.Investor {
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB.Investor;

	[DataContract]
	public class InvestorActionResult : ActionResult {
		[DataMember]
		public InvestorModel Investor { get; set; }
	}
}
