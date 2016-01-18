namespace Ezbob.Backend.ModelsWithDB.ApplicationInfo {
	using System.Runtime.Serialization;

	[DataContract]
	public class AutomationOfferModel {
		[DataMember]
		public int Amount { get; set; }

		[DataMember]
		public decimal InterestRate { get; set; }

		[DataMember]
		public int RepaymentPeriod { get; set; }

		[DataMember]
		public decimal SetupFeePercent { get; set; }

		[DataMember]
		public decimal SetupFeeAmount { get; set; }
	} // class AutomationOfferModel
} // namespace
