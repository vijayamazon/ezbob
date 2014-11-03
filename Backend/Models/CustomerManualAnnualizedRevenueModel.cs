namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;

	[DataContract]
	public class CustomerManualAnnualizedRevenue {
		[DataMember]
		public decimal? Revenue { get; set; }

		[DataMember]
		public DateTime? EntryTime { get; set; }

		[DataMember]
		public string Comment { get; set; }
	} // class Coin
} // namespace
