namespace MaamClient {
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.ProcessHistory.Trails;
	using Ezbob.Database;
	using Ezbob.Logger;
	using OfficeOpenXml;

	internal static class ExportApprovalData {
		public static void Run(string[] args, AConnection db, ASafeLog log) {
			if (args.Length < 1) {
				log.Alert("Usage: MaamClient.exe <DecisionTrail tag>");
				return;
			} // if

			SortedDictionary<long, OneRowData> rows = LoadInputData(db, args[0]);

			LoadRuleDecisions(db, args[0], rows);

			var excel = new ExcelPackage();

			var sheet = excel.CreateSheet("data", OneRowData.GetColumnNames());

			int rowNum = 2;

			foreach (var pair in rows) {
				pair.Value.ToExcel(sheet, rowNum);
				rowNum++;
			} // for each

			excel.AutoFitColumns();

			excel.SaveAs(new FileInfo(@"c:\temp\" + args[0] + ".xlsx"));
		} // InputDataToExcel

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private class OneRowData {
			public long TrailID { get; set; }
			public string ManualDecision { get; set; }
			public int ManuallyApprovedSum { get; set; }
			public string AutoDecision { get; set; }

			static OneRowData() {
				RuleNames = new SortedSet<string>();
			} // static constructor

			public OneRowData(SafeReader sr) {
				sr.Fill(this);
				this.aid = ApprovalInputData.Deserialize(sr["InputData"]);
				RuleDecisions = new SortedList<string, string>();
			} // constructor

			public static string[] GetColumnNames() {
				var lst = new List<string> {
					"TrailId",
					"ManualDecision",
					"ManuallyApprovedSum",
					"AutoDecision",
					"CompanyName",
					"Cfg_ExperianScoreThreshold",
					"Cfg_CustomerMinAge",
					"Cfg_CustomerMaxAge",
					"Cfg_MinTurnover1M",
					"Cfg_MinTurnover3M",
					"Cfg_MinTurnover1Y",
					"Cfg_MinMPSeniorityDays",
					"Cfg_MaxOutstandingOffers",
					"Cfg_MaxTodayLoans",
					"Cfg_MaxDailyApprovals",
					"Cfg_MaxAllowedDaysLate",
					"Cfg_MaxNumOfOutstandingLoans",
					"Cfg_MinRepaidPortion",
					"Cfg_MinAmount",
					"Cfg_MaxAmount",
					"Cfg_IsSilent",
					"Cfg_SilentTemplateName",
					"Cfg_SilentToAddress",
					"Cfg_BusinessScoreThreshold",
					"Cfg_TurnoverDropQuarterRatio",
					"Cfg_OnlineTurnoverAge",
					"Cfg_OnlineTurnoverDropQuarterRatio",
					"Cfg_OnlineTurnoverDropMonthRatio",
					"Cfg_HmrcTurnoverAge",
					"Cfg_HmrcTurnoverDropQuarterRatio",
					"Cfg_HmrcTurnoverDropHalfYearRatio",
					"Cfg_AllowedCaisStatusesWithLoan",
					"Cfg_AllowedCaisStatusesWithoutLoan",
					"CustomerID",
					"CustomerName",
					"DataAsOf",
					"DirectorNames",
					"HmrcBusinessNames",
					"Turnover1Y",
					"Turnover3M",
					"LatePayments",
					"MarketplaceSeniority",
					"Medal",
					"MedalType",
					"TurnoverType",
					"Meta_FirstName",
					"Meta_LastName",
					"Meta_IsBrokerCustomer",
					"Meta_IsLimitedCompanyType",
					"Meta_NumOfTodayAutoApproval",
					"Meta_TodayLoanSum",
					"Meta_FraudStatusValue",
					"Meta_AmlResult",
					"Meta_CustomerStatusName",
					"Meta_CustomerStatusEnabled",
					"Meta_CompanyScore",
					"Meta_ConsumerScore",
					"Meta_IncorporationDate",
					"Meta_DateOfBirth",
					"Meta_NumOfDefaultAccounts",
					"Meta_NumOfRollovers",
					"Meta_TotalLoanCount",
					"Meta_OpenLoanCount",
					"Meta_TakenLoanAmount",
					"Meta_RepaidPrincipal",
					"Meta_SetupFees",
					"Meta_ExperianCompanyName",
					"Meta_EnteredCompanyName",
					"Meta_OutstandingPrincipal",
					"Meta_RepaidRatio",
					"ReservedFunds",
					"SystemCalculatedAmount",
					"WorstStatuses"
				};

				lst.AddRange(RuleNames);

				return lst.ToArray();
			} // GetColumnNames

			public void AddRule(string name, string status) {
				RuleNames.Add(name);
				RuleDecisions[name] = status;
			} // AddRule

			public void ToExcel(ExcelWorksheet sheet, int rowNum) {
				int cellNum = InputDataToExcel(sheet, rowNum);
				RulesToExcel(sheet, rowNum, cellNum);
			} // ToExcel

			private static SortedSet<string> RuleNames { get; set; }

			private SortedList<string, string> RuleDecisions { get; set; }

			private int InputDataToExcel(ExcelWorksheet sheet, int rowNum) {
				int cellNum = 1;

				cellNum = sheet.SetCellValue(rowNum, cellNum, TrailID);
				cellNum = sheet.SetCellValue(rowNum, cellNum, ManualDecision);
				cellNum = sheet.SetCellValue(rowNum, cellNum, ManuallyApprovedSum);
				cellNum = sheet.SetCellValue(rowNum, cellNum, AutoDecision);

				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.CompanyName);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.ExperianScoreThreshold);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.CustomerMinAge);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.CustomerMaxAge);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.MinTurnover1M);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.MinTurnover3M);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.MinTurnover1Y);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.MinMPSeniorityDays);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.MaxOutstandingOffers);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.MaxTodayLoans);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.MaxDailyApprovals);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.MaxAllowedDaysLate);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.MaxNumOfOutstandingLoans);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.MinRepaidPortion);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.MinAmount);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.MaxAmount);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.IsSilent);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.SilentTemplateName);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.SilentToAddress);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.BusinessScoreThreshold);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.TurnoverDropQuarterRatio);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.OnlineTurnoverAge);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.OnlineTurnoverDropQuarterRatio);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.OnlineTurnoverDropMonthRatio);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.HmrcTurnoverAge);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.HmrcTurnoverDropQuarterRatio);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.HmrcTurnoverDropHalfYearRatio);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.AllowedCaisStatusesWithLoan);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Configuration.AllowedCaisStatusesWithoutLoan);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.CustomerID);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.CustomerName.ToString());
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.DataAsOf);
				cellNum = sheet.SetCellValue(rowNum, cellNum, string.Join("|", this.aid.DirectorNames.Select(x => x.ToString())));
				cellNum = sheet.SetCellValue(rowNum, cellNum, string.Join("|", this.aid.HmrcBusinessNames));
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Turnover1Y);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Turnover3M);
				cellNum = sheet.SetCellValue(rowNum, cellNum, string.Join("|", this.aid.LatePayments.Select(p => p.Stringify())));
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MarketplaceSeniority);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.Medal.ToString());
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MedalType.ToString());
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.TurnoverType == null ? string.Empty : this.aid.TurnoverType.Value.ToString());
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.FirstName);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.LastName);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.IsBrokerCustomer);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.IsLimitedCompanyType);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.NumOfTodayAutoApproval);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.TodayLoanSum);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.FraudStatusValue);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.AmlResult);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.CustomerStatusName);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.CustomerStatusEnabled);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.CompanyScore);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.ConsumerScore);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.IncorporationDate == null ? string.Empty : this.aid.MetaData.IncorporationDate.Value.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture));
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.DateOfBirth.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture));
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.NumOfDefaultAccounts);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.NumOfRollovers);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.TotalLoanCount);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.OpenLoanCount);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.TakenLoanAmount);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.RepaidPrincipal);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.SetupFees);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.ExperianCompanyName);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.EnteredCompanyName);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.OutstandingPrincipal);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.MetaData.RepaidRatio);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.ReservedFunds);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.SystemCalculatedAmount);
				cellNum = sheet.SetCellValue(rowNum, cellNum, this.aid.WorstStatuses);

				return cellNum;
			} // InputDataToExcel

			private string GetStatus(string name) {
				return RuleDecisions.ContainsKey(name) ? RuleDecisions[name] : string.Empty;
			} // GetStatus

			private void RulesToExcel(ExcelWorksheet sheet, int rowNum, int cellNum) {
				foreach (string name in RuleNames)
					cellNum = sheet.SetCellValue(rowNum, cellNum, GetStatus(name));
			} // RulesToExcel

			private readonly ApprovalInputData aid;
		} // class OneRowData

		private static SortedDictionary<long, OneRowData> LoadInputData(AConnection db, string tag) {
			string inputDataQuery = @"
SELECT
	t.TrailID,
	d.DecisionName + ' ' + s.DecisionStatus AS AutoDecision,
	r.UnderwriterDecision AS ManualDecision,
	CASE WHEN r.UnderwriterDecision = 'Approved' THEN r.ManagerApprovedSum ELSE 0 END AS ManuallyApprovedSum,
	t.InputData
FROM
	DecisionTrail t
	INNER JOIN DecisionStatuses s ON t.DecisionStatusID = s.DecisionStatusID
	INNER JOIN Decisions d ON t.DecisionID = d.DecisionID
	INNER JOIN CashRequests r ON t.CashRequestID = r.Id
WHERE
	t.Tag = '" + tag + "'";

			SortedDictionary<long, OneRowData> rows = new SortedDictionary<long, OneRowData>();

			db.ForEachRowSafe(
				sr => {
					var ord = new OneRowData(sr);
					rows[ord.TrailID] = ord;
				},
				inputDataQuery,
				CommandSpecies.Text
				);

			return rows;
		} // LoadInputData

		private static void LoadRuleDecisions(AConnection db, string tag, SortedDictionary<long, OneRowData> rows) {
			string query = @"
SELECT
	t.TrailID,
	tc.Name,
	s.DecisionStatus
FROM
	DecisionTrail t
	INNER JOIN DecisionTrace tc ON t.TrailID = tc.TrailID
	INNER JOIN DecisionStatuses s ON tc.DecisionStatusID = s.DecisionStatusID
WHERE
	t.Tag = '" + tag + "'";

			db.ForEachRowSafe(
				sr => {
					long trailID = sr["TrailID"];
					string name = sr["Name"];
					string status = sr["DecisionStatus"];

					rows[trailID].AddRule(name, status);
				},
				query,
				CommandSpecies.Text
				);
		} // LoadRuleDecisions
	} // class ExportApprovalData
} // namespace
