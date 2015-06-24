namespace Reports.UiReports {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using EZBob.DatabaseLib.Model.Database;
	using MainAppReferences;
	using Reports.UiReportsExt;

	public class CustomerInfo : CustomerData {
		public CustomerInfo(
			IDataRecord oRow,
			SortedDictionary<int, AddressInfo> oAddressList,
			SortedDictionary<int, int> oDirectorCountList,
			SortedDictionary<int, int> oAccountCountList
		) : base(oRow) {
			Gender = Retrieve("Gender");
			DateOfBirth = Retrieve<DateTime>("DateOfBirth");
			MaritalStatus = Retrieve("MaritalStatus");
			MobilePhone = Retrieve("MobilePhone");
			DaytimePhone = Retrieve("DaytimePhone");
			TimeAtAddress = Retrieve<int>("TimeAtAddress");
			PropertyStatusDescription = Retrieve("Description");
			CompanyName = Retrieve("CompanyName");
			Origin = Retrieve("Origin");

			WizardStep = Retrieve<int>("WizardStep").Value;

			AddressInfo = oAddressList.ContainsKey(ID) ? oAddressList[ID] : null;
			DirectorCount = oDirectorCountList.ContainsKey(ID) ? oDirectorCountList[ID] : 0;
			AccountCount = oAccountCountList.ContainsKey(ID) ? oAccountCountList[ID] : 0;
		} // constructor

		public string Gender { get; private set; }
		public DateTime? DateOfBirth { get; private set; }
		public string MaritalStatus { get; private set; }
		public string MobilePhone { get; private set; }
		public string DaytimePhone { get; private set; }
		public int? TimeAtAddress { get; private set; }
		public string PropertyStatusDescription { get; private set; }
		public string CompanyName { get; private set; }
		public int WizardStep { get; private set; }
		public string Origin { get; private set; }

		public AddressInfo AddressInfo { get; private set; }
		public int DirectorCount { get; private set; }
		public int AccountCount { get; private set; }

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

			case UiItemGroups.LinkAccounts:
				return AccountCount > 0;
			} // switch

			throw new ArgumentOutOfRangeException("nItemGroup");
		} // HasAllData

		public override string ToString() {
			return string.Format(
				"{17} {13}: {12} {0} {1} {2}, born on {3}, currently {4}, available at {5} or {6}, " +
				"residental age is {7}, is a {8}, business {9}, company name {10} " +
				"addresses {14}, director count {15}, account count {16}",
				NameTitle(),
				Value(FirstName),
				Value(Surname),
				Value(DateOfBirth),
				Value(MaritalStatus),
				Value(MobilePhone),
				Value(DaytimePhone),
				Value(TimeAtAddress),
				Value(PropertyStatusDescription),
				Value(TypeOfBusiness),
				Value(CompanyName),
				"",
				Segment(),
				ID,
				Value(AddressInfo),
				DirectorCount,
				AccountCount,
				Origin
			);
		} // ToString

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

		private bool HasCompanyDetails() {
			TypeOfBusiness bt;

			if (!EZBob.DatabaseLib.Model.Database.TypeOfBusiness.TryParse(TypeOfBusiness, out bt))
				return false;

			switch (bt) {
			case EZBob.DatabaseLib.Model.Database.TypeOfBusiness.Entrepreneur:
				return true;

			case EZBob.DatabaseLib.Model.Database.TypeOfBusiness.LLP:
			case EZBob.DatabaseLib.Model.Database.TypeOfBusiness.Limited:
			case EZBob.DatabaseLib.Model.Database.TypeOfBusiness.PShip3P:
			case EZBob.DatabaseLib.Model.Database.TypeOfBusiness.PShip:
			case EZBob.DatabaseLib.Model.Database.TypeOfBusiness.SoleTrader:
				return !string.IsNullOrWhiteSpace(CompanyName);

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // HasCompanyDetails

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
	} // class CustomerInfo
} // namespace
