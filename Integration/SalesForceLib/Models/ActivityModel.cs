namespace SalesForceLib.Models {
	using System;
	using System.Runtime.Serialization;
	
	[DataContract(IsReference = true)]
	public class ActivityModel {
		[DataMember]
		public string Email { get; set; }
		//----------------------------------------//
		[DataMember]
		public DateTime StartDate { get; set; }
		[DataMember]
		public DateTime EndDate { get; set; }
		[DataMember]
		public string Type { get; set; }
		[DataMember]
		public string Description { get; set; }
		[DataMember]
		public string Originator { get; set; }
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
