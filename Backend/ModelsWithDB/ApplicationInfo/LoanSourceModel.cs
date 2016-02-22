namespace Ezbob.Backend.ModelsWithDB.ApplicationInfo {
	using System.Runtime.Serialization;

	[DataContract]
	public class LoanSourceModel {
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public decimal MaxInterest { get; set; }

		[DataMember]
		public int DefaultRepaymentPeriod { get; set; }

		[DataMember]
		public bool IsCustomerRepaymentPeriodSelectionAllowed { get; set; }

		[DataMember]
		public int MaxEmployeeCount { get; set; }

		[DataMember]
		public decimal MaxAnnualTurnover { get; set; }

		[DataMember]
		public bool IsDefault { get; set; }

		[DataMember]
		public int AlertOnCustomerReasonType { get; set; }
	} // class LoanSourceModel
} // namespace
