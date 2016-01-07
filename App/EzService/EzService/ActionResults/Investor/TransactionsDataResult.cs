namespace EzService.ActionResults.Investor {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models.Investor;
	using System.Collections.Generic;

	[DataContract]
	public class TransactionsDataResult : ActionResult {
		[DataMember]
		public List<TransactionsDataModel> TransactionsData { get; set; }
	}
}



