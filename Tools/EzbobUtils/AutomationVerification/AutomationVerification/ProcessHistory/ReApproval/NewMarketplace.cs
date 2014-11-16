namespace AutomationCalculator.ProcessHistory.ReApproval {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Newtonsoft.Json;

	public class NewMarketplace : ATrace {
		#region constructor

		public NewMarketplace(int nCustomerID, DecisionStatus nDecisionStatus) : base(nCustomerID, nDecisionStatus) {
		} // constructor

		#endregion constructor

		#region method Init

		public void Init(int nID, string sName, string sType, DateTime oAddTime) {
			MpID = nID;
			MpName = sName;
			MpType = sType;
			MpAddTime = oAddTime;

			if (MpID == 0)
				Comment = string.Format("customer {0} has not added marketplaces after the last manually approved cash request", CustomerID);
			else {
				Comment = string.Format(
					"customer {0} has added a marketplace {1}({2} - {3}) on {4}",
					CustomerID,
					MpName,
					MpID,
					MpType,
					MpAddTime.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
				);
			} // if
		} // Init

		#endregion method Init

		public int MpID { get; private set; }
		public string MpName { get; private set; }
		public string MpType { get; private set; }
		public DateTime MpAddTime { get; private set; }

		public override string GetInitArgs() {
			return JsonConvert.SerializeObject(new List<object> { MpID, MpName, MpType, MpAddTime });
		} // GetInitArgse
	} // class NewMarketplace
} // namespace
