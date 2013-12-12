using System;
using System.Collections.Generic;
using System.Data;
using EZBob.DatabaseLib.Model.Database;

namespace Reports {
	#region class CustomerInfo

	public class CustomerInfo : Extractor {
		#region public

		#region constructor

		public CustomerInfo(IDataRecord oRow, SortedDictionary<int, AddressInfo> oAddressList, SortedDictionary<int, int> oDirectorCountList) : base(oRow) {
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

			AddressInfo = oAddressList.ContainsKey(ID) ? oAddressList[ID] : null;
			DirectorCount = oDirectorCountList.ContainsKey(ID) ? oDirectorCountList[ID] : 0;
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

		public AddressInfo AddressInfo { get; private set; }
		public int DirectorCount { get; private set; }

		#endregion properties

		#region method HasAllData

		public bool HasAllData(UiItemGroups nItemGroup) {
			switch (nItemGroup) {
			case UiItemGroups.PersonalInfo:
				return !string.IsNullOrWhiteSpace(FirstName);

			case UiItemGroups.HomeAddress:
				return HasHomeAddress();

			case UiItemGroups.ContactDetails:
				return !string.IsNullOrWhiteSpace(MobilePhone);

			case UiItemGroups.CompanyInfo:
				return !string.IsNullOrWhiteSpace(TypeOfBusiness);

			case UiItemGroups.CompanyDetails:
				return HasCompanyDetails();

			case UiItemGroups.AdditionalDirectors:
				return DirectorCount > 0;

			default:
				throw new ArgumentOutOfRangeException("nItemGroup");
			} // switch

			return false;
		} // HasAllData

		#endregion method HasAllData

		#region method ToString

		public override string ToString() {
			return string.Format(
				"{13}: {12} {0} {1} {2}, born on {3}, currently {4}, available at {5} or {6}, " +
				"residual age is {7}, is a {8}, business {9}, unlim name {10}, ltd name {11} " +
				"addresses {14}, director count {15}",
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
				ID,
				Value(AddressInfo),
				DirectorCount
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

		#region method HasCompanyDetails

		private bool HasCompanyDetails() {
			EZBob.DatabaseLib.Model.Database.TypeOfBusiness bt;

			if (!EZBob.DatabaseLib.Model.Database.TypeOfBusiness.TryParse(TypeOfBusiness, out bt))
				return false;

			switch (bt) {
			case EZBob.DatabaseLib.Model.Database.TypeOfBusiness.Entrepreneur:
				return true;

			case EZBob.DatabaseLib.Model.Database.TypeOfBusiness.LLP:
			case EZBob.DatabaseLib.Model.Database.TypeOfBusiness.Limited:
				return !string.IsNullOrWhiteSpace(LimitedCompanyName);

			case EZBob.DatabaseLib.Model.Database.TypeOfBusiness.PShip3P:
			case EZBob.DatabaseLib.Model.Database.TypeOfBusiness.PShip:
			case EZBob.DatabaseLib.Model.Database.TypeOfBusiness.SoleTrader:
				return !string.IsNullOrWhiteSpace(NonLimitedCompanyName);

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // HasCompanyDetails

		#endregion method HasCompanyDetails

		#region method HasHomeAddress

		private bool HasHomeAddress() {
			if (AddressInfo == null)
				return false;

			if (AddressInfo[CustomerAddressType.PersonalAddress] < 1)
				return false;

			if (!TimeAtAddress.HasValue)
				return false;

			if ((TimeAtAddress == 1) || (TimeAtAddress == 2))
				return AddressInfo[CustomerAddressType.PrevPersonAddresses] > 0;

			return true;
		} // HasHomeAddress

		#endregion method HasHomeAddress

		#endregion private
	} // class CustomerInfo

	#endregion class CustomerInfo
} // namespace Reports
