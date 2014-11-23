namespace EzBob.Backend.Strategies.AutomationVerification {
	using System.Collections.Generic;
	using Ezbob.Database;

	internal class AutoApproveInputRow {
		public int CustomerId { get; set; }
		public string Medal { get; set; }
		public int OfferedLoanAmount { get; set; }

		public static List<AutoApproveInputRow> Load(
			AConnection oDB,
			int nTopCount,
			int nLastCheckedCustomerID
		) {
			string sTop = (nTopCount > 0) ? "TOP " + nTopCount : string.Empty;

			string sCondition = (nLastCheckedCustomerID > 0)
				? "AND mc.CustomerId < " + nLastCheckedCustomerID
				: string.Empty;

			const string sQueryFormat = @"
SELECT {0}
	mc.CustomerId,
	mc.Medal,
	ISNULL(mc.OfferedLoanAmount, 0) as OfferedLoanAmount
FROM
	MedalCalculationsAV mc
WHERE
	mc.IsActive = 1
	{1}
ORDER BY mc.CustomerId DESC";

			return oDB.Fill<AutoApproveInputRow>(
				string.Format(sQueryFormat, sTop, sCondition),
				CommandSpecies.Text
			);
		} // Load
	} // class AutoApproveInputRow
} // namespace
