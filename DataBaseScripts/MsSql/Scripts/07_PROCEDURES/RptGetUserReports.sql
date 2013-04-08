IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptGetUserReports]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptGetUserReports]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptGetUserReports
	@UserName    	 NVARCHAR(50)
AS
BEGIN
DECLARE @UserId INT = (SELECT Id FROM ReportUsers WHERE UserName=@UserName)
SELECT rs.*
FROM ReportsUsersMap rum, ReportScheduler rs, ReportUsers ru
WHERE ru.UserName = @UserName
AND ru.Id = rum.UserID
AND rs.Id = rum.ReportID
END
GO
