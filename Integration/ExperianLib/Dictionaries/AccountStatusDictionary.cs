namespace ExperianLib.Dictionaries {
	using System.Collections.Generic;

	public static class AccountStatusDictionary {
		#region static constructor

		static AccountStatusDictionary() {
			ms_oAccountStatuses = new SortedDictionary<string, string> {
				{"0", "OK"},
				{"1", "30"},
				{"2", "60"},
				{"3", "90"},
				{"4", "120"},
				{"5", "150"},
				{"6", "180"},
				{"8", "Def"},
				{"9", "Bad"},
				{"S", "Slow"},
				{"U", "U"},
				{"D", "Dorm"},
				{"?", "?"},
			};

			ms_oAccountDetailedStatuses = new SortedDictionary<string, string> {
				{"0", "0 days"},
				{"1", "30 days"},
				{"2", "60 days"},
				{"3", "90 days"},
				{"4", "120 days"},
				{"5", "150 days"},
				{"6", "180 days"},
				{"8", "Default"},
				{"9", "Bad Debt"},
				{"S", "Slow Payer"},
				{"U", "Unclassified"},
				{"D", "Dormant"},
				{"?", "Unknown"},
			};

			ms_oAccountStatusColors = new SortedDictionary<string, string> {
				{"0", "info"},
				{"1", "warning"},
				{"2", "warning"},
				{"3", "warning"},
				{"4", "warning"},
				{"5", "warning"},
				{"6", "warning"},
				{"8", "danger"},
				{"9", "danger"},
				{"S", "warning"},
				{"U", "warning"},
				{"D", "warning"},
				{"?", "warning"},
			};
		} // static constructor

		#endregion static constructor

		#region method GetAccountStatusString

		public static string GetAccountStatusString(string accStatusIndicator) {
			return LookFor(accStatusIndicator, ms_oAccountStatuses);
		} // GetAccountStatusString

		public static string GetAccountStatusColor(string accStatusIndicator)
		{
			return LookFor(accStatusIndicator, ms_oAccountStatusColors);
		} // GetAccountStatusString

		#endregion method GetAccountStatusString

		#region method GetDetailedAccountStatusString

		public static string GetDetailedAccountStatusString(string accStatusIndicator) {
			return LookFor(accStatusIndicator, ms_oAccountDetailedStatuses);
		} // GetDetailedAccountStatusString

		#endregion method GetDetailedAccountStatusString

		#region private

		#region method LookFor

		private static string LookFor(string sNeedle, SortedDictionary<string, string> oHaystack) {
			return oHaystack.ContainsKey(sNeedle) ? oHaystack[sNeedle] : (sNeedle ?? string.Empty);
		} // LookFor

		#endregion method LookFor

		private static readonly SortedDictionary<string, string> ms_oAccountStatuses;
		private static readonly SortedDictionary<string, string> ms_oAccountDetailedStatuses;
		private static readonly SortedDictionary<string, string> ms_oAccountStatusColors;

		#endregion private
	} // class AccountStatusDictionary
} // namespace
