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
		} // static constructor

		#endregion static constructor

		#region method GetAccountStatusString

		public static string GetAccountStatusString(string accStatusIndicator) {
			return LookFor(accStatusIndicator, ms_oAccountStatuses);
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

		#endregion private
	} // class AccountStatusDictionary
} // namespace
