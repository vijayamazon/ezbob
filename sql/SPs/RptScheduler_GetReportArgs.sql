IF OBJECT_ID('RptScheduler_GetReportArgs') IS NULL
	EXECUTE('CREATE PROCEDURE RptScheduler_GetReportArgs AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptScheduler_GetReportArgs
@RptType NVARCHAR(200) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		r.Id AS ReportID,
		r.Type AS ReportType,
		n.Id AS ArgumentID,
		n.Name AS ArgumentName
	FROM
		ReportScheduler r
		INNER JOIN ReportArguments a ON r.Id = a.ReportId
		INNER JOIN ReportArgumentNames n ON a.ReportArgumentNameId = n.Id
	WHERE
		@RptType IS NULL
		OR
		r.Type = @RptType
	ORDER BY
		r.Type,
		n.Name
END
GO
