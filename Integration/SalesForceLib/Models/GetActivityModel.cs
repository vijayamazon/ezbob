namespace SalesForceLib.Models {
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class GetActivityModel {
		[DataMember]
		public string Email { get; set; } // lead/account/opportunity unique identifier
		[DataMember]
		public string Origin { get; set; } // lead/account/opportunity unique identifier
	}
}
