namespace AutomationCalculator.ProcessHistory.ReApproval {
	using System;
	using System.Globalization;

	public class NewMarketplace : ATrace {

		public NewMarketplace(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		public void Init(bool newMpWasAdded) {
			Comment = newMpWasAdded
						  ? string.Format("customer has added a new marketplace")
				          : string.Format("customer has not added marketplaces after the last manually approved cash request");
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

		public int MpID { get; private set; }
		public string MpName { get; private set; }
		public string MpType { get; private set; }
		public DateTime MpAddTime { get; private set; }
	} // class NewMarketplace
} // namespace
