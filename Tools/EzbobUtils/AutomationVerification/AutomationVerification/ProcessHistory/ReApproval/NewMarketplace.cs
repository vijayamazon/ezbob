namespace AutomationCalculator.ProcessHistory.ReApproval {
	using System;
	using System.Globalization;

	public class NewMarketplace : ATrace {
		#region constructor

		public NewMarketplace(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		#endregion constructor

		#region method Init

		public void Init(bool newMpWasAdded) {
			Comment = newMpWasAdded
						  ? string.Format("customer {0} has added a new marketplace", CustomerID)
				          : string.Format("customer {0} has not added marketplaces after the last manually approved cash request", CustomerID);
		}

		public void Init(int nID, string sName, string sType, DateTime oAddTime) {
			MpID = nID;
			MpName = sName;
			MpType = sType;
			MpAddTime = oAddTime;

			if (MpID == 0)
				Comment = string.Format("customer has not added marketplaces after the last manually approved cash request");
			else {
				Comment = string.Format(
					"customer has added a marketplace {1}({2} - {3}) on {0}",
					MpAddTime.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
					MpName,
					MpID,
					MpType
				);
			} // if
		} // Init

		#endregion method Init

		public int MpID { get; private set; }
		public string MpName { get; private set; }
		public string MpType { get; private set; }
		public DateTime MpAddTime { get; private set; }
	} // class NewMarketplace
} // namespace
