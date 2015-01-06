namespace SalesForceLib.Models {
	using System;
	//todo use auto generated object from sales force
	public class EventModel {
		public string Email { get; set; }
		//----------------------------------------//
		public DateTime Date { get; set; }
		public string Type { get; set; }
		public string Desciption { get; set; }
		public string Originator { get; set; }
	}

	public enum EventType {
		Email,
		Imail,
		Sms,
		Note,
		Call,
		Chat
	}
}
