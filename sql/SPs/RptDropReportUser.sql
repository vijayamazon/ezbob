IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptDropReportUser]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptDropReportUser]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptDropReportUser] 
	(@UserID INT)
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
