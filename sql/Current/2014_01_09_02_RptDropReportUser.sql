IF OBJECT_ID('RptDropReportUser') IS NULL
	EXECUTE('CREATE PROCEDURE RptDropReportUser AS SELECT 1')
GO

ALTER PROCEDURE RptDropReportUser
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

