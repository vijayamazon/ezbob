IF EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'Environment' AND Value = 'Prod')
BEGIN
	DECLARE @RoleId INT
	SELECT @RoleId = RoleId FROM Security_Role WHERE Name = 'Collector'
	INSERT INTO Security_UserRoleRelation (UserId, RoleId) VALUES (2561, @RoleId) -- Emma
	
	SELECT @RoleId = RoleId FROM Security_Role WHERE Name = 'Sales'
	INSERT INTO Security_UserRoleRelation (UserId, RoleId) VALUES (4916, @RoleId) -- Ros
	INSERT INTO Security_UserRoleRelation (UserId, RoleId) VALUES (16110, @RoleId) -- Clare
	
	SELECT @RoleId = RoleId FROM Security_Role WHERE Name = 'BrokerSales'
	INSERT INTO Security_UserRoleRelation (UserId, RoleId) VALUES (10127, @RoleId) -- Travis
END
GO
