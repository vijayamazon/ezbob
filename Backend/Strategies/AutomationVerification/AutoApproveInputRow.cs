namespace EzBob.Backend.Strategies.AutomationVerification {
	using System.Collections.Generic;
	using Ezbob.Database;

	internal class AutoApproveInputRow {
		public int CustomerId { get; set; }
		public string Medal { get; set; }
		public int OfferedLoanAmount { get; set; }

		public static List<AutoApproveInputRow> Load(AConnection oDB, int nTopCount) {
			string sTop = (nTopCount > 0) ? "TOP " + nTopCount : string.Empty;

			string sQuery = string.Format(
				"SELECT {0} mc.CustomerId, mc.Medal, ISNULL(mc.OfferedLoanAmount, 0) as OfferedLoanAmount " +
				"FROM MedalCalculationsAV mc WHERE mc.IsActive = 1 ORDER BY mc.CustomerId",
				sTop
			);

			return oDB.Fill<AutoApproveInputRow>(sQuery, CommandSpecies.Text);
		} // Load
	} // class AutoApproveInputRow
} // namespace
