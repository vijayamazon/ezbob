namespace SalesForceLib.Models {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	
	[DataContract(IsReference = true)]
	public class GetActivityResultModel {
		public GetActivityResultModel() {
			Activities = new List<ActivityResultModel>();
		}
		public GetActivityResultModel(IEnumerable<ActivityResultModel> activities, string error) {
			Activities = activities;
			Error = error;
		}

		[DataMember]
		public IEnumerable<ActivityResultModel> Activities { get; set; } 

		[DataMember]
		public string Error { get; set; }
	}

	[DataContract(IsReference = true)]
	public class ActivityResultModel {
		[DataMember]
		public DateTime? StartDate { get; set; }
		[DataMember]
		public DateTime? EndDate { get; set; }
		[DataMember]
		public string Type { get; set; }
		[DataMember]
		public string Description { get; set; }
		[DataMember]
		public string Subject { get; set; }
		[DataMember]
		public string Originator { get; set; }
		[DataMember]
		public string Priority { get; set; }
		[DataMember]
		public string Place { get; set; }
		[DataMember]
		public string Status { get; set; }
		[DataMember]
		public string Creator { get; set; }
	}
}
