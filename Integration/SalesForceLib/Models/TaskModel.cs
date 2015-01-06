namespace SalesForceLib.Models {
	using System;

	//todo use auto generated object from sales force
	public class TaskModel {
		public string Email { get; set; }
		public DateTime CreateDate { get; set; }
		public DateTime DueDate{ get; set; }
		public string Originator{ get; set; }
		public string Subject{ get; set; }
		public string Status { get; set; } // ? no updates only create with default value
	}
}
