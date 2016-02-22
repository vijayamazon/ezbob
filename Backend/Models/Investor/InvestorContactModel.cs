namespace Ezbob.Backend.Models.Investor {
	using System;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class InvestorContactModel {
		[DataMember]
		public int InvestorContactID { get; set; }

		[DataMember]
		public int InvestorID { get; set; }

		[DataMember]
		public string PersonalName { get; set; }

		[DataMember]
		public string LastName { get; set; }

		[DataMember]
		public string Email { get; set; }

		[DataMember]
		public string Role { get; set; }

		[DataMember]
		public string Comment { get; set; }

		[DataMember]
		public bool IsPrimary { get; set; }

		[DataMember]
		public string Mobile { get; set; }

		[DataMember]
		public string OfficePhone { get; set; }

		[DataMember]
		public bool IsActive { get; set; }

        [DataMember]
        public bool IsGettingAlerts { get; set; }
     
        [DataMember]
        public bool IsGettingReports { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }
	}
}
