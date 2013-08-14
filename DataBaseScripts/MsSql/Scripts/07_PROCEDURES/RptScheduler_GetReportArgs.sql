IF OBJECT_ID('RptScheduler_GetReportArgs') IS NOT NULL
	DROP PROC RptScheduler_GetReportArgs
GO

CREATE PROCEDURE RptScheduler_GetReportArgs
@RptType NVARCHAR(200) = NULL
AS
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
GO
