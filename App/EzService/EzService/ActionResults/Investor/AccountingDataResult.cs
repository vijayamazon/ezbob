namespace EzService.ActionResults.Investor {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models.Investor;
	using System.Collections.Generic;

	[DataContract]
	public class AccountingDataResult : ActionResult {
		[DataMember]
		public List<AccountingDataModel> AccountingData { get; set; }
	}
}
