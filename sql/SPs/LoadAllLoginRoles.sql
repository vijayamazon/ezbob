IF OBJECT_ID('LoadAllLoginRoles') IS NULL
	EXECUTE('CREATE PROCEDURE LoadAllLoginRoles AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE LoadAllLoginRoles
@Login VARCHAR(250)
AS
BEGIN
	SELECT DISTINCT
		r.RoleId,
		r.Name
	FROM
		Security_Role r
		INNER JOIN Security_UserRoleRelation ur ON r.RoleId = ur.RoleId
		INNER JOIN Security_User u ON ur.UserId = u.UserId
	WHERE
		LOWER(u.UserName) = @Login
END
GO
