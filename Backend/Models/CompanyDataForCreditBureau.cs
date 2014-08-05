namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;

	[DataContract]
	public class CompanyDataForCreditBureau {
		[DataMember]
		public DateTime? LastUpdate { get; set; }

		[DataMember]
		public int Score { get; set; }

		[DataMember]
		public string Errors { get; set; }
	} // class CompanyDataForCreditBureau
} // namespace
