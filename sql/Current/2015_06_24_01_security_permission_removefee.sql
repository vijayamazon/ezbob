DECLARE @id INT;
SET @id = (SELECT TOP 1 Id FROM Security_Permission ORDER BY Id DESC);
SET @id = @id + 1;

IF NOT EXISTS (SELECT 1 FROM [Security_Permission] WHERE Name='RemoveFee')
BEGIN
	INSERT INTO [Security_Permission] VALUES (@id,'RemoveFee',NULL)
END

DECLARE @roleId INT;
SET @roleID = (SELECT  RoleId FROM Security_Role WHERE Name='SuperUser');

IF NOT EXISTS (SELECT 1 FROM [Security_RolePermissionRel] WHERE RoleId=@roleID AND PermissionId=@id)
BEGIN
	INSERT INTO [Security_RolePermissionRel] VALUES (@roleID,@id)
END
