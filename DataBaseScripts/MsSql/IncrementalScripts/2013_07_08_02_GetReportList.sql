IF OBJECT_ID('RptScheduler_GetReportList') IS NOT NULL
	DROP PROCEDURE RptScheduler_GetReportList
GO

CREATE PROCEDURE RptScheduler_GetReportList
@RptType NVARCHAR(200) = NULL
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
	FROM
		ReportScheduler
	WHERE
		@RptType IS NULL OR LTRIM(RTRIM(@RptType)) = ''
		OR
		Type = @RptType
END
GO

