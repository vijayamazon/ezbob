using System;
using System.Collections.Generic;

namespace Reports {
	#region enum UiItemGroups

	public enum UiItemGroups {
		PersonalInfo = 0,
		HomeAddress = 1,
		ContactDetails = 2,
		CompanyInfo = 3,
		CompanyDetails = 4,
		AdditionalDirectors = 5,
	} // enum UiItemGroups

	#endregion enum UiItemGroups

	#region class UiItemGroupsSequence

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

	#endregion class UiItemGroupsSequence
} // namespace Reports
