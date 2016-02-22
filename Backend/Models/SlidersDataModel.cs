namespace Ezbob.Backend.Models {
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class SlidersDataModel {

		[DataMember]
		public decimal? Amount { get; set; }

		[DataMember]
		public int? Term { get; set; }

		[DataMember]
		public decimal MinLoanAmount { get; set; }

		[DataMember]
		public decimal MaxLoanAmount { get; set; }

		[DataMember]
		public int MinTerm { get; set; }

		[DataMember]
		public int MaxTerm { get; set; }
	}
}


