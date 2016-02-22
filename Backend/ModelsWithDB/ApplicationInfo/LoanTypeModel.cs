namespace Ezbob.Backend.ModelsWithDB.ApplicationInfo {
	using System.Runtime.Serialization;

	[DataContract]
	public class LoanTypeModel {
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public string Name { get; set; }

		// ReSharper disable once InconsistentNaming
		[DataMember]
		public int value { get; set; }

		// ReSharper disable once InconsistentNaming
		[DataMember]
		public string text { get; set; }

		[DataMember]
		public int RepaymentPeriod { get; set; }
	} // class LoanTypeModel
} // namespace
