IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptChangePassword]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptChangePassword]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptChangePassword
	@UserName NVARCHAR(50)
   ,@Password VARBINARY(20)
   ,@Salt     VARBINARY(20) 
AS 
BEGIN
	UPDATE ReportUsers SET Password=@Password, Salt=@Salt WHERE UserName=@UserName
END
GO
