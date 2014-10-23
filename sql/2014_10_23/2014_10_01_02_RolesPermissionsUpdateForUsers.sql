IF EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'Environment' AND Value = 'Prod')
BEGIN
	DECLARE @RoleId INT
	
	SELECT @RoleId = RoleId FROM Security_Role WHERE Name = 'Collector'	
	IF NOT EXISTS (SELECT 1 FROM Security_UserRoleRelation WHERE UserId = 2561 AND RoleId = @RoleId)
		INSERT INTO Security_UserRoleRelation (UserId, RoleId) VALUES (2561, @RoleId) -- Emma
	
	SELECT @RoleId = RoleId FROM Security_Role WHERE Name = 'Sales'
	IF NOT EXISTS (SELECT 1 FROM Security_UserRoleRelation WHERE UserId = 4916 AND RoleId = @RoleId)
		INSERT INTO Security_UserRoleRelation (UserId, RoleId) VALUES (4916, @RoleId) -- Ros
	IF NOT EXISTS (SELECT 1 FROM Security_UserRoleRelation WHERE UserId = 16110 AND RoleId = @RoleId)
		INSERT INTO Security_UserRoleRelation (UserId, RoleId) VALUES (16110, @RoleId) -- Clare
	
	SELECT @RoleId = RoleId FROM Security_Role WHERE Name = 'BrokerSales'
	IF NOT EXISTS (SELECT 1 FROM Security_UserRoleRelation WHERE UserId = 10127 AND RoleId = @RoleId)
		INSERT INTO Security_UserRoleRelation (UserId, RoleId) VALUES (10127, @RoleId) -- Travis
END
GO
