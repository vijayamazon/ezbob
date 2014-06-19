namespace EchoSignLib {
	internal class Person : AAddressable {
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }

		public string FullName {
			get { return (FirstName.Trim() + " " + LastName.Trim()).Trim(); } // get
		} // FullName

		public string FullDetails {
			get { return FullName + ", " + FullAddress; } // get
		} // FullDetails
	} // class Person
} // namespace
