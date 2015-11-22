namespace EzService.ActionResults.Investor {
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[DataContract]
	public class InvestorTypesActionResult : ActionResult {
		[DataMember]
		public IDictionary<string, string> InvestorTypes { get; set; }
		[DataMember]
		public IDictionary<string, string> InvestorBankAccountTypes { get; set; } 
	}
}
