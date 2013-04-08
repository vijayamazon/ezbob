IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptAddUserReportMap]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptAddUserReportMap]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptAddUserReportMap
	@UserName    	 NVARCHAR(50)
   ,@ReportType      NVARCHAR(200)
AS
BEGIN
DECLARE @UserId INT = (SELECT Id FROM ReportUsers WHERE UserName=@UserName)
DECLARE @ReportId INT = (SELECT Id FROM ReportScheduler WHERE ReportScheduler.Type=@ReportType)
INSERT INTO ReportsUsersMap (UserID, ReportID) VALUES (@UserId, @ReportId)
END
GO
