using System;
using System.Data;

namespace Reports {
	#region class CustomerInfo

	public class CustomerInfo : Extractor {
		#region public

		#region constructor

		public CustomerInfo(IDataRecord oRow) : base(oRow) {
			ID = Retrieve<int>("CustomerID").Value;
			FirstName = Retrieve("FirstName");
			Surname = Retrieve("Surname");
			Gender = Retrieve("Gender");
			DateOfBirth = Retrieve<DateTime>("DateOfBirth");
			MaritalStatus = Retrieve("MaritalStatus");
			MobilePhone = Retrieve("MobilePhone");
			DaytimePhone = Retrieve("DaytimePhone");
			TimeAtAddress = Retrieve<int>("TimeAtAddress");
			ResidentialStatus = Retrieve("ResidentialStatus");
			TypeOfBusiness = Retrieve("TypeOfBusiness");
			NonLimitedCompanyName = Retrieve("NonLimitedCompanyName");
			LimitedCompanyName = Retrieve("LimitedCompanyName");

			IsOffline = Convert.ToBoolean(oRow["IsOffline"]);
		} // constructor

		#endregion constructor

		#region properties

		public int ID { get; private set; }
		public string FirstName { get; private set; }
		public string Surname { get; private set; }
		public string Gender { get; private set; }
		public DateTime? DateOfBirth { get; private set; }
		public string MaritalStatus { get; private set; }
		public string MobilePhone { get; private set; }
		public string DaytimePhone { get; private set; }
		public int? TimeAtAddress { get; private set; }
		public string ResidentialStatus { get; private set; }
		public string TypeOfBusiness { get; private set; }
		public string NonLimitedCompanyName { get; private set; }
		public string LimitedCompanyName { get; private set; }
		public bool IsOffline { get; private set; }

		#endregion properties

		#region method ToString

		public override string ToString() {
			return string.Format(
				"{13}: {12} {0} {1} {2}, born on {3}, currently {4}, available at {5} or {6}, " +
				"residual age is {7}, is a {8}, business {9}, unlim name {10}, ltd name {11}",
				NameTitle(),
				Value(FirstName),
				Value(Surname),
				Value(DateOfBirth),
				Value(MaritalStatus),
				Value(MobilePhone),
				Value(DaytimePhone),
				Value(TimeAtAddress),
				Value(ResidentialStatus),
				Value(TypeOfBusiness),
				Value(NonLimitedCompanyName),
				Value(LimitedCompanyName),
				Segment(),
				ID
			);
		} // ToString

		#endregion method ToString

		#endregion public

		#region private

		#region method Segment

		private string Segment() {
			return IsOffline ? "Offline" : "Online";
		} // Segment

		#endregion method Segment

		#region method NameTitle

		private string NameTitle() {
			switch (Gender) {
			case "M":
				return "Lord";
			case "F":
				return "Lady";
			default:
				return "some";
			} // switch
		} // NameTitle

		#endregion method NameTitle

		#endregion private
	} // class CustomerInfo

	#endregion class CustomerInfo
} // namespace Reports
