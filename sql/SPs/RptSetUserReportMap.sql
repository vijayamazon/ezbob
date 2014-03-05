IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptSetUserReportMap]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptSetUserReportMap]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptSetUserReportMap] 
	(@UserID INT,
@ReportID INT,
@Enabled BIT)
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
