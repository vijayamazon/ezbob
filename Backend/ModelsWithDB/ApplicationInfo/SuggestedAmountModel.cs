namespace Ezbob.Backend.ModelsWithDB.ApplicationInfo {
	using System.ComponentModel;
	using System.Runtime.Serialization;

	public enum CalculationMethod {
		[Description("FCF")]
		FCF,

		[Description("Revenue")]
		Turnover,

		[Description("Value added")]
		ValueAdded,

		[Description("Manual")]
		Manual,
	} // enum CalculationMethod

	[DataContract]
	public class SuggestedAmountModel {
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public string Method { get; set; }

		[DataMember]
		public decimal Silver { get; set; }

		[DataMember]
		public decimal Gold { get; set; }

		[DataMember]
		public decimal Diamond { get; set; }

		[DataMember]
		public decimal Platinum { get; set; }

		[DataMember]
		public decimal Value { get; set; }

		public SuggestedAmountModel() {
			Id = -1;
		} // constructor
	} // class SuggestedAmountModel
} // namespace
