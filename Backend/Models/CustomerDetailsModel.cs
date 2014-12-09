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

		public override string ToString() {
			return string.Format("{0} {1} {2} {3}", CustomerID, FirstName, LastName, Email);
		} // ToString
	} // class CustomerDetails
} // namespace Ezbob.Backend.Strategies.Broker
