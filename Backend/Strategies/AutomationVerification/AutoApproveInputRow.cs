namespace Ezbob.Backend.Strategies.AutomationVerification {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;

	public class AutoApproveInputRow {
		public int CustomerId { get; set; }
		public string MedalStr { get; set; }
		public string MedalTypeStr { get; set; }
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
	MedalStr = mc.Medal,
	MedalTypeStr = mc.MedalType,
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

		public Medal GetMedal() {
			Medal nMedal;

			if (!Enum.TryParse(MedalStr, out nMedal))
				nMedal = Medal.NoClassification;

			return nMedal;
		} // GetMedal

		public AutomationCalculator.Common.MedalType GetMedalType() {
			AutomationCalculator.Common.MedalType nMedalType;

			if (!Enum.TryParse(MedalTypeStr, out nMedalType))
				nMedalType = AutomationCalculator.Common.MedalType.NoMedal;

			return nMedalType;
		} // GetMedalType
	} // class AutoApproveInputRow
} // namespace
