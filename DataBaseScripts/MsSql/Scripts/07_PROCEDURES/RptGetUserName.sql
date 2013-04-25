IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptGetUserName]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptGetUserName]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptGetUserName
	@UserName    	 NVARCHAR(50)
AS
BEGIN
SELECT Name FROM ReportUsers WHERE UserName=@UserName
END
GO
