namespace EzBob.Backend.Models {
	using System.Runtime.Serialization;
	using Ezbob.Utils;

	[DataContract]
	public enum ConfigTableType
	{
		[EnumMember]
		LoanOfferMultiplier,

		[EnumMember]
		BasicInterestRate,

		[EnumMember]
		EuLoanMonthlyInterest,

		[EnumMember]
		DefaultRateCompany,

		[EnumMember]
		DefaultRateCustomer,
	}

	[DataContract]
	public class ConfigTable : ITraversable
	{
		[DataMember]
		[NonTraversable]
		public int Id { get; set; }
		
		[DataMember]
		public int Start { get; set; }

		[DataMember]
		public int End { get; set; }

		[DataMember]
		public decimal Value { get; set; }
	}
}