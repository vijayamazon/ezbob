IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'ChangePhone')
BEGIN

	DECLARE @lastPermission INT = (SELECT max(Id)+1 FROM Security_Permission) 
	
	INSERT INTO dbo.Security_Permission	(Id, Name)
	VALUES	( @lastPermission, 'ChangePhone')
		
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId)
	SELECT RoleId, @lastPermission FROM Security_Role WHERE RoleId IN (31,32,34,35,37, 38)
END
GO