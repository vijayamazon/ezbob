namespace SalesForceLib.Models {
	using System;
	using System.Runtime.Serialization;

	//todo use auto generated object from sales force
	[DataContract]
	public class TaskModel {
		[DataMember]
		public string Email { get; set; }
		[DataMember]
		public DateTime CreateDate { get; set; }
		[DataMember]
		public DateTime DueDate { get; set; }
		[DataMember]
		public string Originator { get; set; }
		[DataMember]
		public string Subject { get; set; }
		[DataMember]
		public string Status { get; set; } // ? no updates only create with default value
	}
}
