namespace Ezbob.Backend.Models {
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class CustomerDetails {
		[DataMember]
		public int CustomerID { get; set; }

		[DataMember]
		public string FirstName { get; set; }

		[DataMember]
		public string LastName { get; set; }

		[DataMember]
		public string Email { get; set; }
	} // class CustomerDetails
} // namespace EzBob.Backend.Strategies.Broker
