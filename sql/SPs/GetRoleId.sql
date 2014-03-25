IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetRoleId]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetRoleId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetRoleId] 
	(@Name NVARCHAR(255))
AS
BEGIN
	SELECT 
		RoleId 
	FROM 
		Security_Role 
	WHERE 
		Name = @Name
END
GO
