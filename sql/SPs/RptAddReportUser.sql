IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptAddReportUser]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptAddReportUser]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptAddReportUser] 
	(@UserName NVARCHAR(50),
@Name     NVARCHAR(50),
@Password VARBINARY(20),
@Salt     VARBINARY(20))
AS
BEGIN
	INSERT INTO ReportUsers (UserName, Name, Password, Salt)
		VALUES (@UserName, @Name, @Password, @Salt)
END
GO
