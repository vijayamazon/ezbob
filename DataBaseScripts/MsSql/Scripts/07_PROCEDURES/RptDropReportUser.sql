IF OBJECT_ID('RptDropReportUser') IS NOT NULL
	DROP PROCEDURE RptDropReportUser
GO

CREATE PROCEDURE RptDropReportUser
@UserID INT
AS
BEGIN
	DELETE FROM
		ReportsUsersMap
	WHERE
		UserID = @UserID

	DELETE FROM
		ReportUsers
	WHERE
		Id = @UserID
END
GO
