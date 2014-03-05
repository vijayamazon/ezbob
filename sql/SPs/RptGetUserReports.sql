IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptGetUserReports]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptGetUserReports]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptGetUserReports] 
	(@UserName NVARCHAR(50) = NULL)
AS
BEGIN
	DECLARE @UserId INT = (SELECT Id FROM ReportUsers WHERE UserName = @UserName)
	
	SELECT
		rs.Id,
		rs.Type,
		rs.Title,
		rs.StoredProcedure,
		rs.IsDaily,
		rs.IsWeekly,
		rs.IsMonthly,
		rs.Header,
		rs.Fields,
		rs.ToEmail,
		rs.IsMonthToDate
	FROM
		ReportScheduler rs
		INNER JOIN ReportsUsersMap rum ON rs.Id = rum.ReportID
		INNER JOIN ReportUsers ru ON rum.UserID = ru.Id
	WHERE
		@UserName IS NULL
		OR
		ru.UserName = @UserName
	ORDER BY
		rs.Title
END
GO
