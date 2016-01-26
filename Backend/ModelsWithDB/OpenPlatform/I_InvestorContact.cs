namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_InvestorContact {
		//[PK] not identity 
        [DataMember]
		public int InvestorContactID { get; set; }

		[FK("I_Investor", "InvestorID")]
        [DataMember]
        public int InvestorID { get; set; }
		
		[Length(255)]
		[DataMember]
		public string PersonalName { get; set; }

		[Length(255)]
		[DataMember]
		public string LastName { get; set; }

		[Length(255)]
		[DataMember]
		public string Email { get; set; }
		
		[Length(255)]
		[DataMember]
		public string Role { get; set; }

		[Length(255)]
		[DataMember]
		public string Comment { get; set; }

		[DataMember]
		public bool IsPrimary { get; set; }

		[Length(30)]
		[DataMember]
		public string Mobile { get; set; }

		[Length(30)]
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
	}//class InvestorContact
}//ns
		