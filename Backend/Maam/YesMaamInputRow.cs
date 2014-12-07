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

		public int CollectionStatus { get; set; }

		public int UnderwriterID { get; set; }
		public string UnderwriterName { get; set; }

		public string MedalType { get; set; }

		public int Amount { get; set; }

		public Medal Medal {
			get {
				if (m_nMedal == Medal.NoClassification)
					Enum.TryParse(MedalType, out m_nMedal);

				return m_nMedal;
			} // get
		} // Medal

		private Medal m_nMedal;

		public YesMaamInputRow() {
			m_nMedal = Medal.NoClassification;
		} // constructor

		public static List<YesMaamInputRow> Load(
			AConnection oDB,
			int nTopCount,
			int nLastCheckedID
		) {
			string sTop = (nTopCount > 0) ? "TOP " + nTopCount : string.Empty;

			string sCondition = (nLastCheckedID > 0)
				? "AND r.Id < " + nLastCheckedID
				: string.Empty;

			const string sQueryFormat = @"
SELECT {0}
	r.Id AS CashRequestID,
	r.IdCustomer AS CustomerID,
	r.UnderwriterDecisionDate,
	r.UnderwriterDecision,
	c.CollectionStatus,
	r.IdUnderwriter AS UnderwriterID,
	u.UserName AS UnderwriterName,
	r.MedalType,
	ISNULL(dbo.udfMaxInt(r.SystemCalculatedSum, r.ManagerApprovedSum), 100000) AS Amount
FROM
	CashRequests r
	INNER JOIN Customer c ON r.IdCustomer = c.Id
	INNER JOIN Security_User u ON r.IdUnderwriter = u.UserId
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

			return oDB.Fill<YesMaamInputRow>(
				string.Format(sQueryFormat, sTop, sCondition),
				CommandSpecies.Text
			);
		} // Load
	} // class AutoApproveInputRow
} // namespace
