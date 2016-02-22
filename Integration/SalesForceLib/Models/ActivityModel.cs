namespace SalesForceLib.Models {
	using System;
	using System.Runtime.Serialization;
	
	[DataContract(IsReference = true)]
	public class ActivityModel {
		[DataMember]
		public string Email { get; set; } // lead/account/opportunity unique identifier
		[DataMember]
		public string Origin { get; set; } // lead/account/opportunity unique identifier
		//----------------------------------------//
		[DataMember]
		public DateTime StartDate { get; set; }
		[DataMember]
		public DateTime EndDate { get; set; }
		[DataMember]
		public string Type { get; set; } //ActivityType enum
		[DataMember]
		public string Description { get; set; }
		[DataMember]
		public string Originator { get; set; }
		[DataMember]
		public bool IsOpportunity { get; set; }
	}

	public enum ActivityType {
		Email,
		Imail,
		Sms,
		Note,
		Call,
		Chat
	}
}
