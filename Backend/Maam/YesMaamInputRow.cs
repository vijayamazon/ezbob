namespace Ezbob.Maam {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.Common;
	using Ezbob.Database;

	public class YesMaamInputRow {
		public int CashRequestID { get; set; }
		public int CustomerID { get; set; }

		[FieldName("UnderwriterDecisionDate")]
		public DateTime DecisionTime { get; set; }

		[FieldName("UnderwriterDecision")]
		public string Decision { get; set; }

		public string CollectionStatus { get; set; }

		public int UnderwriterID { get; set; }
		public string UnderwriterName { get; set; }

		public string MedalType { get; set; }

		public int Amount { get; set; }

		public int ApprovedAmount { get; set; }

		public Medal Medal {
			get {
				if (this.medal == Medal.NoClassification)
					Enum.TryParse(MedalType, out this.medal);

				return this.medal;
			} // get
		} // Medal

		private Medal medal;

		public YesMaamInputRow() {
			this.medal = Medal.NoClassification;
		} // constructor

		public static List<YesMaamInputRow> Load(
			AConnection oDB,
			int topCount,
			int lastCheckedID
		) {
			return Load(oDB, topCount, lastCheckedID, 0);
		} // Load

		public static List<YesMaamInputRow> Load(AConnection oDB, int cashRequestID) {
			return Load(oDB, 0, 0, cashRequestID);
		} // Load

		private static List<YesMaamInputRow> Load(
			AConnection db,
			int topCount,
			int lastCheckedID,
			int cashRequestID
		) {
			string filter = cashRequestID > 0 ? "r.Id = " + cashRequestID + " AND" : string.Empty;

			var rawList = db.Fill<YesMaamInputRow>(string.Format(QueryFormat, filter), CommandSpecies.Text);

			var byCustomer = new SortedDictionary<int, List<YesMaamInputRow>>();

			foreach (YesMaamInputRow ymir in rawList) {
				if (!byCustomer.ContainsKey(ymir.CustomerID)) {
					byCustomer[ymir.CustomerID] = new List<YesMaamInputRow> { ymir };
					continue;
				} // if

				List<YesMaamInputRow> customerData = byCustomer[ymir.CustomerID];

				var lastKnown = customerData.Last();

				// EZ-3048: from two cash requests that happen in less than 24 hours only the latest should be taken.
				if ((ymir.DecisionTime - lastKnown.DecisionTime).TotalHours < 24)
					customerData.RemoveAt(customerData.Count - 1);

				customerData.Add(ymir);
			} // for each

			var result = new List<YesMaamInputRow>();

			foreach (List<YesMaamInputRow> lst in byCustomer.Values)
				result.AddRange(lst);

			result.Sort((a, b) => b.CashRequestID.CompareTo(a.CashRequestID));

			if (lastCheckedID > 0)
				result = result.Where(ymir => ymir.CashRequestID < lastCheckedID).ToList();

			if (topCount > 0)
				result = result.Take(topCount).ToList();

			return result;
		} // Load

		private const string QueryFormat = @"
SELECT
	r.Id AS CashRequestID,
	r.IdCustomer AS CustomerID,
	r.UnderwriterDecisionDate,
	r.UnderwriterDecision,
	cs.Name AS CollectionStatus,
	r.IdUnderwriter AS UnderwriterID,
	u.UserName AS UnderwriterName,
	r.MedalType,
	ISNULL(dbo.udfMaxInt(r.SystemCalculatedSum, r.ManagerApprovedSum), 100000) AS Amount,
	ISNULL(r.ManagerApprovedSum, 0) AS ApprovedAmount
FROM
	CashRequests r
	INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	INNER JOIN Security_User u ON r.IdUnderwriter = u.UserId
	INNER JOIN CustomerStatuses cs ON c.CollectionStatus = cs.Id
WHERE {0}
	r.IdUnderwriter IS NOT NULL
	AND
	r.UnderwriterDecision IN ('Approved', 'Rejected')
	AND
	r.IdUnderwriter IS NOT NULL
	AND
	r.IdUnderwriter != 1
ORDER BY
	r.Id DESC
";

	} // class AutoApproveInputRow
} // namespace
