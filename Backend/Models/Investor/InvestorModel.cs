namespace Ezbob.Backend.Models.Investor {
	using System;
	using System.Runtime.Serialization;

	[DataContract]
	public class InvestorModel {
		[DataMember]
		public int InvestorID { get; set; }

		[DataMember]
		public int InvestorTypeID { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public bool IsActive { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }
	}
}
