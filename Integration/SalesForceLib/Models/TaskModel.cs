namespace SalesForceLib.Models {
	using System;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class TaskModel {
		public TaskModel() {
			Priority = PriorityEnum.Normal.ToString();
			var now = DateTime.UtcNow;
			CreateDate = now;
			DueDate = now.AddDays(7);
		}

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
		[DataMember]
		public string Priority { get; set; }
	}

	public enum PriorityEnum {
		High,
		Normal,
		Low
	}
}
