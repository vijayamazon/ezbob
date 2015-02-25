namespace Ezbob.Backend.Models.ExternalAPI {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Newtonsoft.Json;

	[DataContract]
	public class AvailableCreditResult {

		[DataMember]
		public string CustomerEmail { get; set; }

		[DataMember]
		[NonTraversable]
		public int CustomerID { get; set; }

		[DataMember]
		public string Decision { get; set; }

		[DataMember]
		public string DecisionDate { get; set; }

		[DataMember]
		public decimal Amount { get; set; }

		[DataMember]
		public string Comment { get; set; }

		[DataMember]
		public DateTime OfferStart { get; set; }

		[DataMember]
		public DateTime OfferValidUntil { get; set; }

		[DataMember]
		[NonTraversable]
		public int AliId { get; set; }

		[DataMember]
		[NonTraversable]
		public int AlibabaId { get; set; }

		public override string ToString() {
			return JsonConvert.SerializeObject(this);
		}
	}
}
