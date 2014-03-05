IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptScheduler_GetReportArgs]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptScheduler_GetReportArgs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptScheduler_GetReportArgs] 
	(@RptType NVARCHAR(200) = NULL)
AS
BEGIN
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
