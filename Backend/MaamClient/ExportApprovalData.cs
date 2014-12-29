namespace MaamClient {
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

			var excel = new ExcelPackage();

			var sheet = excel.CreateSheet("data",
				"TrailId",
				"ManualDecision",
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
				);

			string query = @"
SELECT
	t.TrailID,
	d.DecisionName + ' ' + s.DecisionStatus AS AutoDecision,
	r.UnderwriterDecision AS ManualDecision,
	t.InputData
FROM
	DecisionTrail t
	INNER JOIN DecisionStatuses s ON t.DecisionStatusID = s.DecisionStatusID
	INNER JOIN Decisions d ON t.DecisionID = d.DecisionID
	INNER JOIN CashRequests r ON t.CashRequestID = r.Id
WHERE
	t.Tag = '" + args[0] + "'";

			int rowNum = 2;

			db.ForEachRowSafe(
				sr => {
					int cellNum = 1;

					ApprovalInputData aid = ApprovalInputData.Deserialize(sr["InputData"]);

					cellNum = sheet.SetCellValue(rowNum, cellNum, (long)sr["TrailID"]);
					cellNum = sheet.SetCellValue(rowNum, cellNum, (string)sr["ManualDecision"]);
					cellNum = sheet.SetCellValue(rowNum, cellNum, (string)sr["AutoDecision"]);

					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.CompanyName);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.ExperianScoreThreshold);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.CustomerMinAge);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.CustomerMaxAge);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.MinTurnover1M);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.MinTurnover3M);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.MinTurnover1Y);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.MinMPSeniorityDays);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.MaxOutstandingOffers);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.MaxTodayLoans);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.MaxDailyApprovals);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.MaxAllowedDaysLate);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.MaxNumOfOutstandingLoans);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.MinRepaidPortion);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.MinAmount);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.MaxAmount);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.IsSilent);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.SilentTemplateName);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.SilentToAddress);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.BusinessScoreThreshold);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.TurnoverDropQuarterRatio);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.OnlineTurnoverAge);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.OnlineTurnoverDropQuarterRatio);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.OnlineTurnoverDropMonthRatio);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.HmrcTurnoverAge);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.HmrcTurnoverDropQuarterRatio);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.HmrcTurnoverDropHalfYearRatio);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.AllowedCaisStatusesWithLoan);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Configuration.AllowedCaisStatusesWithoutLoan);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.CustomerID);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.CustomerName.ToString());
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.DataAsOf);
					cellNum = sheet.SetCellValue(rowNum, cellNum, string.Join("|", aid.DirectorNames.Select(x => x.ToString())));
					cellNum = sheet.SetCellValue(rowNum, cellNum, string.Join("|", aid.HmrcBusinessNames));
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Turnover1Y);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Turnover3M);
					cellNum = sheet.SetCellValue(rowNum, cellNum, string.Join("|", aid.LatePayments.Select(p => p.Stringify())));
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MarketplaceSeniority);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.Medal.ToString());
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MedalType.ToString());
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.TurnoverType == null ? string.Empty : aid.TurnoverType.Value.ToString());
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.FirstName);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.LastName);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.IsBrokerCustomer);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.IsLimitedCompanyType);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.NumOfTodayAutoApproval);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.TodayLoanSum);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.FraudStatusValue);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.AmlResult);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.CustomerStatusName);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.CustomerStatusEnabled);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.CompanyScore);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.ConsumerScore);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.IncorporationDate == null ? string.Empty : aid.MetaData.IncorporationDate.Value.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture));
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.DateOfBirth.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture));
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.NumOfDefaultAccounts);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.NumOfRollovers);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.TotalLoanCount);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.OpenLoanCount);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.TakenLoanAmount);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.RepaidPrincipal);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.SetupFees);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.ExperianCompanyName);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.EnteredCompanyName);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.OutstandingPrincipal);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.MetaData.RepaidRatio);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.ReservedFunds);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.SystemCalculatedAmount);
					cellNum = sheet.SetCellValue(rowNum, cellNum, aid.WorstStatuses);

					rowNum++;
				},
				query,
				CommandSpecies.Text
				);

			excel.AutoFitColumns();

			excel.SaveAs(new FileInfo(@"c:\temp\" + args[0] + ".xlsx"));
		} // InputDataToExcel
	} // class ExportApprovalData
} // namespace
