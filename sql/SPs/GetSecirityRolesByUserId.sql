IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSecirityRolesByUserId]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetSecirityRolesByUserId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetSecirityRolesByUserId] 
	(@iUserId bigint)
AS
BEGIN
	select r.RoleId, r.Name, r.Description
	from Security_Role r
	left join Security_UserRoleRelation rr on rr.RoleId = r.RoleId
	where rr.UserId = @iUserId
END
GO
