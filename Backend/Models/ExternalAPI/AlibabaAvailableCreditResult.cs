namespace Ezbob.Backend.Models.ExternalAPI {
	using System;
	using System.Runtime.Serialization;
	using Newtonsoft.Json;

	[DataContract]
	public class AlibabaAvailableCreditResult {

		[DataMember(EmitDefaultValue = true)]
		public int? aliMemberId { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public int? aId { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public string currency = "GBP";

		[DataMember(EmitDefaultValue = true)]
		public int? loanId { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public decimal? unusedCreditAmount { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public decimal? unusedCreditAmount_USD { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public DateTime? lastUpdate { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public decimal? creditLine { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public decimal? creditLine_USD { get; set; }

		[DataMember(EmitDefaultValue = true)]
		public string lineStatus { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public DateTime? OfferStart { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public DateTime? OfferValidUntil { get; set; }

		public override string ToString() {
			return JsonConvert.SerializeObject(this);
		}
	}
}
