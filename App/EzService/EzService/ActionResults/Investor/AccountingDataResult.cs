namespace EzService.ActionResults.Investor {
	using System.Runtime.Serialization;
	using System.Collections.Generic;
	using Ezbob.Backend.Models.Investor;

	[DataContract]
	public class AccountingDataResult : ActionResult {
		[DataMember]
		public List<AccountingDataModel> AccountingData { get; set; }
	}
}
