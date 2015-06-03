namespace SalesForceLib.Models {
	using System;
	using System.Runtime.Serialization;
	
	[DataContract(IsReference = true)]
	public class GetActivityResultModel {
		[DataMember]
		public string Email { get; set; } // lead/account/opportunity unique identifier
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
}
