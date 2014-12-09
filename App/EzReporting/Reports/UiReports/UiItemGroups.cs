using System;
using System.Collections.Generic;

namespace Reports {

	// Values of the enum members are used to order data in the output table.
	// New enum member can be inserted in the middle.
	// However if enum contains n members they must cover all the numbers
	// from 0 to n-1 inclusive, see ms_SortedGroups initialisation in the
	// static constructor below.
	public enum UiItemGroups {
		PersonalInfo = 0,
		HomeAddress = 1,
		ContactDetails = 2,
		LinkAccounts = 3,
		CompanyInfo = 4,
		CompanyDetails = 5,
		AdditionalDirectors = 6,
	} // enum UiItemGroups

	public static class UiItemGroupsSequence {
		static UiItemGroupsSequence() {
			ms_SortedGroups = new List<UiItemGroups>();

			for (int i = 0; Enum.IsDefined(typeof (UiItemGroups), i); i++)
				ms_SortedGroups.Add((UiItemGroups)i);
		} // static UiItemGroupsSequence

		public static List<UiItemGroups> Get() {
			return ms_SortedGroups;
		} // Get

		private static readonly List<UiItemGroups> ms_SortedGroups;
	} // class UiItemGroupsSequence

} // namespace Reports
