namespace Ezbob.Maam {
	using System;
	using System.Collections.Generic;
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
			string top = (topCount > 0) ? "TOP " + topCount : string.Empty;

			string condition = (lastCheckedID > 0)
				? "AND r.Id < " + lastCheckedID
				: string.Empty;

			return Load(oDB, top, condition);
		} // Load

		public static List<YesMaamInputRow> Load(AConnection oDB, int cashRequestID) {
			return Load(oDB, string.Empty, "AND r.Id = " + cashRequestID);
		} // Load

		private static List<YesMaamInputRow> Load(AConnection db, string top, string condition) {
			const string sQueryFormat = @"
SELECT {0}
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
WHERE
	r.IdUnderwriter IS NOT NULL
	AND
	r.UnderwriterDecision IN ('Approved', 'Rejected')
	AND
	r.IdUnderwriter IS NOT NULL
	AND
	r.IdUnderwriter != 1
	{1}
ORDER BY
	r.Id DESC
";

			return db.Fill<YesMaamInputRow>(
				string.Format(sQueryFormat, top, condition),
				CommandSpecies.Text
			);
		} // Load
	} // class AutoApproveInputRow
} // namespace
