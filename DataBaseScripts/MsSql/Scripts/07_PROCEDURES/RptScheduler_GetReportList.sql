IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptScheduler_GetReportList]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptScheduler_GetReportList]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
