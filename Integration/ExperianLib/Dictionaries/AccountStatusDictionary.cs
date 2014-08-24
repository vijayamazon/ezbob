namespace ExperianLib.Dictionaries {

	using System.Collections.Generic;

	public class CaisStatus {
		public string ShortDescription { get; set; }
		public string LongDescription { get; set; }
		public string Color { get; set; }
		public bool IsLate { get; set; }
		public bool IsDefault { get; set; }
	}

	public static class AccountStatusDictionary {
		static AccountStatusDictionary() {
			ms_oAccountStatuses = new SortedDictionary<string, CaisStatus> {
				{" ",new CaisStatus{ShortDescription = "&nbsp;", LongDescription = "Empty", Color = "white"}},
				{"0", new CaisStatus{ShortDescription = "OK", LongDescription = "0 days", Color = "info"} },
				{"1",new CaisStatus{ShortDescription = "30", LongDescription = "30 days", Color = "warning", IsLate = true} },
				{"2", new CaisStatus{ShortDescription = "60", LongDescription = "60 days", Color = "warning", IsLate = true}},
				{"3", new CaisStatus{ShortDescription = "90", LongDescription = "90 days", Color = "warning", IsLate = true}},
				{"4", new CaisStatus{ShortDescription = "120", LongDescription = "120 days", Color = "warning", IsLate = true}},
				{"5",new CaisStatus{ShortDescription = "150", LongDescription = "150 days", Color = "warning", IsLate = true} },
				{"6", new CaisStatus{ShortDescription = "180", LongDescription = "180 days", Color = "warning", IsLate = true}},
				{"8", new CaisStatus{ShortDescription = "Def", LongDescription = "Default", Color = "danger", IsDefault = true}},
				{"9", new CaisStatus{ShortDescription = "Bad", LongDescription = "Bad Debt", Color = "danger", IsDefault = true}},
				{"S", new CaisStatus{ShortDescription = "Slow", LongDescription = "Slow Payer", Color = "warning"}},
				{"U",new CaisStatus{ShortDescription = "U", LongDescription = "Unclassified", Color = "warning"} },
				{"D", new CaisStatus{ShortDescription = "Dorm", LongDescription = "Dormant", Color = "warning"}},
				{"?", new CaisStatus{ShortDescription = "Unknown", LongDescription = "Unknown", Color = "warning"}},
			};
		} // static constructor

		public static CaisStatus GetAccountStatus(string accStatusIndicator) {
			return LookFor(accStatusIndicator, ms_oAccountStatuses);
		} // GetAccountStatusString


		private static CaisStatus LookFor(string sNeedle, SortedDictionary<string, CaisStatus> oHaystack) {
			if (string.IsNullOrEmpty(sNeedle)) {
				return new CaisStatus(){ LongDescription = string.Format("Status: ({0}) not found", sNeedle)};
			}
			return oHaystack.ContainsKey(sNeedle) ? oHaystack[sNeedle] : new CaisStatus() { LongDescription = string.Format("Status: ({0}) not found", sNeedle) };
		} // LookFor


		private static readonly SortedDictionary<string, CaisStatus> ms_oAccountStatuses;

	} // class AccountStatusDictionary
} // namespace
