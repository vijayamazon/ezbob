IF NOT EXISTS (SELECT 1 FROM Security_Permission WHERE Name='PacnetManualButton')
BEGIN
	DECLARE @NewPermissionId INT,
			@ManagerRoleId INT
	
	SELECT @NewPermissionId = max(Id) + 1 FROM Security_Permission		
			
	INSERT INTO Security_Permission VALUES (@NewPermissionId, 'PacnetManualButton', null)
	
	SELECT @ManagerRoleId = RoleId FROM Security_Role WHERE Name = 'manager'
	INSERT INTO Security_RolePermissionRel VALUES (@ManagerRoleId, @NewPermissionId)
END
GO
