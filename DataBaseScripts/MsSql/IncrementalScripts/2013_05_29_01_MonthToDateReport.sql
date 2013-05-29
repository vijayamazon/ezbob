IF OBJECT_ID('DF_RptSchedule_MonthToDate') IS NULL
	ALTER TABLE ReportScheduler ADD IsMonthToDate BIT CONSTRAINT DF_RptSchedule_MonthToDate DEFAULT (0) NOT NULL
GO

IF OBJECT_ID('RptScheduler_GetReportList') IS NOT NULL
	DROP PROCEDURE RptScheduler_GetReportList
GO

CREATE PROCEDURE RptScheduler_GetReportList
AS
BEGIN
	SELECT
		Type,
		Title,
		StoredProcedure,
		IsDaily,
		IsWeekly,
		IsMonthly,
		IsMonthToDate,
		Header,
		Fields,
		ToEmail
	FROM ReportScheduler
END
GO

