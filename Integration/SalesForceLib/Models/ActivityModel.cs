﻿namespace SalesForceLib.Models {
	using System;
	using System.Runtime.Serialization;

	//todo use auto generated object from sales force
	[DataContract]
	public class ActivityModel {
		[DataMember]
		public string Email { get; set; }
		//----------------------------------------//
		[DataMember]
		public DateTime Date { get; set; }
		[DataMember]
		public string Type { get; set; }
		[DataMember]
		public string Desciption { get; set; }
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