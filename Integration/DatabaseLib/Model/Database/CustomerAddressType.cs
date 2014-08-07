namespace EZBob.DatabaseLib.Model.Database {
	public enum CustomerAddressType {
		PersonalAddress = 1,
		PrevPersonAddresses = 2,
		LimitedCompanyAddress = 3,
		LimitedDirectorHomeAddress = 4,
		NonLimitedCompanyAddress = 5,
		NonLimitedDirectorHomeAddress = 6,
		LimitedCompanyAddressPrev = 7,
		LimitedDirectorHomeAddressPrev = 8,
		NonLimitedCompanyAddressPrev = 9,
		NonLimitedDirectorHomeAddressPrev = 10,
		OtherPropertyAddress = 11,
		OtherPropertyAddressRemoved = 12, // Was 11 in the past. 'No longer owned property'
		ExperianCompanyAddress = 13
	} // enum CustomerAddressType
} // namespace EZBob.DatabaseLib.Model.Database
