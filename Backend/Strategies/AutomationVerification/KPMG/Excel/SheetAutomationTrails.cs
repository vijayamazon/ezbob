namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG.Excel {
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using Ezbob.ExcelExt;
	using OfficeOpenXml;

	internal class SheetAutomationTrails {
		public SheetAutomationTrails(ExcelPackage workbook, SortedDictionary<long, AutomationTrails> automationTrails) {
			var columnNames = new List<string> {
				"Cash request ID",
				"Automation decision"
			};

			columnNames.AddRange(GetApprovalColumnNames());

			this.sheet = workbook.CreateSheet("Automation trails", false, columnNames.ToArray());

			this.automationTrails = automationTrails;

			this.rowNum = 2;
		} // constructor

		public void Generate(long cashRequestID) {
			if (!this.automationTrails.ContainsKey(cashRequestID))
				return;

			AutomationTrails atra = this.automationTrails[cashRequestID];

			int column = this.sheet.SetCellValue(this.rowNum, 1, cashRequestID);
			TrailsToExcel(column, atra);

			this.rowNum++;
		} // Generate

		private void TrailsToExcel(int cellNum, AutomationTrails atra) {
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, atra.AutomationDecision.ToString());

			var aid = atra.Approval.MyInputData;

			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.ExperianScoreThreshold);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.CustomerMinAge);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.CustomerMaxAge);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.MinTurnover1M);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.MinTurnover3M);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.MinTurnover1Y);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.MinMPSeniorityDays);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.MaxOutstandingOffers);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.MaxTodayLoans);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.MaxDailyApprovals);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.MaxAllowedDaysLate);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.MaxNumOfOutstandingLoans);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.MinRepaidPortion);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.MinLoan);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.MaxAmount);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.IsSilent);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.SilentTemplateName);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.SilentToAddress);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.BusinessScoreThreshold);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.TurnoverDropQuarterRatio);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.OnlineTurnoverAge);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.OnlineTurnoverDropQuarterRatio);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.OnlineTurnoverDropMonthRatio);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.HmrcTurnoverAge);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.HmrcTurnoverDropQuarterRatio);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.HmrcTurnoverDropHalfYearRatio);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.AllowedCaisStatusesWithLoan);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Configuration.AllowedCaisStatusesWithoutLoan);
			// cellNum = sheet.SetCellValue(this.rowNum, cellNum, string.Join("|", aid.Configuration.EnabledTraces));
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.CustomerName.ToString());
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.DataAsOf);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, string.Join("|", aid.DirectorNames.Select(x => x.ToString())));
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, string.Join("|", aid.HmrcBusinessNames));
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Turnover1Y);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Turnover3M);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, string.Join("|", aid.LatePayments.Select(p => p.Stringify())));
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MarketplaceSeniority);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.Medal.ToString());
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MedalType.ToString());
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.TurnoverType == null ? string.Empty : aid.TurnoverType.Value.ToString());
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.FirstName);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.LastName);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.IsBrokerCustomer);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.IsLimitedCompanyType);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.NumOfTodayAutoApproval);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.TodayLoanSum);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.FraudStatusValue);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.AmlResult);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.CustomerStatusName);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.CustomerStatusEnabled);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.CompanyScore);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.ConsumerScore);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.IncorporationDate == null ? string.Empty : aid.MetaData.IncorporationDate.Value.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture));
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.DateOfBirth.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture));
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.NumOfDefaultAccounts);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.NumOfRollovers);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.TotalLoanCount);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.OpenLoanCount);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.TakenLoanAmount);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.RepaidPrincipal);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.SetupFees);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.ExperianCompanyName);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.EnteredCompanyName);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.OutstandingPrincipal);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.RepaidRatio);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.MetaData.PreviousManualApproveCount);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.ReservedFunds);
			cellNum = this.sheet.SetCellValue(this.rowNum, cellNum, aid.SystemCalculatedAmount);
		} // TrailsToExcel

		private static IEnumerable<string> GetApprovalColumnNames() {
			string[] lst = {
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
				// "Cfg_EnabledTraces",
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
				"Meta_PreviousManualApproveCount",
				"ReservedFunds",
				"SystemCalculatedAmount",
				"WorstStatuses"
			};

			return lst.Select(s => "Approve-" + s);
		} // GetApprovalColumnNames

		private readonly ExcelWorksheet sheet;
		private readonly SortedDictionary<long, AutomationTrails> automationTrails;
		private int rowNum;
	} // class SheetAutomationTrails
} // namespace
