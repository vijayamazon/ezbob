IF OBJECT_ID('RptSetUserReportMap') IS NULL
	EXECUTE('CREATE PROCEDURE RptSetUserReportMap AS SELECT 1')
GO

ALTER PROCEDURE RptSetUserReportMap
@UserID INT,
@ReportID INT,
@Enabled BIT
AS
BEGIN
	IF @Enabled = 1
	BEGIN
		IF NOT EXISTS (SELECT * FROM ReportsUsersMap WHERE UserID = @UserID AND ReportID = @ReportID)
		BEGIN
			INSERT INTO ReportsUsersMap (UserID, ReportID)
				VALUES (@UserID, @ReportID)
		END
	END
	ELSE BEGIN
		DELETE FROM
			ReportsUsersMap
		WHERE
			UserID = @UserID
			AND
			ReportID = @ReportID
	END
END
GO
