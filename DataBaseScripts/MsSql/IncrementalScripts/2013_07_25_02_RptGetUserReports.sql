IF OBJECT_ID('RptGetUserReports') IS NOT NULL
	DROP PROCEDURE RptGetUserReports
GO

CREATE PROCEDURE RptGetUserReports
@UserName NVARCHAR(50)
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
		ru.UserName = @UserName
	ORDER BY
		rs.Title
END
GO
