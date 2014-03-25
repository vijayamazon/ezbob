IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserIdByName]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetUserIdByName]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetUserIdByName] 
	(@Name NVARCHAR(250))
AS
BEGIN
	SELECT 
		UserId
	FROM 
		Security_User 
	WHERE 
		UserName = @Name
END
GO
