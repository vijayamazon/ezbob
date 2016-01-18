namespace EzService.ActionResults.Investor {
	using System.Runtime.Serialization;
	using System.Collections.Generic;
	using Ezbob.Backend.Models.Investor;

	[DataContract]
	public class TransactionsDataResult : ActionResult {
		[DataMember]
		public List<TransactionsDataModel> TransactionsData { get; set; }
	}
}



