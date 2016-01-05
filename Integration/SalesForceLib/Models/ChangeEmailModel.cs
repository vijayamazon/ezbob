namespace SalesForceLib.Models {
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class ChangeEmailModel {
		[DataMember]
		public string currentEmail { get; set; } // lead/account/opportunity unique identifier
		[DataMember]
		public string Origin { get; set; } // lead/account/opportunity unique identifier
		//----------------------------------------//
		[DataMember]
		public string newEmail { get; set; } 
	}
}
