--adding permissions:  Rollover,MakePayment,LoanOptions,AvoidAutomaticDecision,SuspendBtn,ReturnBtn,DiscountPlan,DisableShop,HideBrokerClientDetails
IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'Rollover')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission	(Id, Name, Description) VALUES (@id, 'Rollover', NULL)
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'MakePayment')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission	(Id, Name, Description) VALUES (@id, 'MakePayment', NULL)
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'LoanOptions')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission	(Id, Name, Description) VALUES (@id, 'LoanOptions', NULL)
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'AvoidAutomaticDecision')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission	(Id, Name, Description) VALUES (@id, 'AvoidAutomaticDecision', NULL)
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'SuspendBtn')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission	(Id, Name, Description) VALUES (@id, 'SuspendBtn', NULL)
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'DiscountPlan')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission	(Id, Name, Description) VALUES (@id, 'DiscountPlan', NULL)
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'DisableShop')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission	(Id, Name, Description) VALUES (@id, 'DisableShop', NULL)
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'HideBrokerClientDetails')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission	(Id, Name, Description) VALUES (@id, 'HideBrokerClientDetails', NULL)
END    
GO

IF NOT EXISTS (SELECT * FROM Security_Permission WHERE Name = 'ReturnBtn')
BEGIN 
	DECLARE @id INT = (SELECT max(Id) FROM Security_Permission) + 1
    INSERT INTO Security_Permission	(Id, Name, Description) VALUES (@id, 'ReturnBtn', NULL)
END    
GO
--removing Nova,Inspector,CreditAnalyst,Maven,Patron,Auditor,FormsDesigner role
IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'Nova')
BEGIN
	DECLARE @roleId INT = (SELECT RoleId FROM Security_Role WHERE Name = 'Nova')
	DELETE FROM Security_RolePermissionRel WHERE RoleId = @roleId
	DELETE FROM Security_UserRoleRelation WHERE RoleId = @roleId
	DELETE FROM Security_Role WHERE RoleId = @roleId
END
GO

IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'Inspector')
BEGIN
	DECLARE @roleId INT = (SELECT RoleId FROM Security_Role WHERE Name = 'Inspector')
	DELETE FROM Security_RolePermissionRel WHERE RoleId = @roleId
	DELETE FROM Security_UserRoleRelation WHERE RoleId = @roleId
	DELETE FROM Security_Role WHERE RoleId = @roleId
END
GO

IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'CreditAnalyst')
BEGIN
	DECLARE @roleId INT = (SELECT RoleId FROM Security_Role WHERE Name = 'CreditAnalyst')
	DELETE FROM Security_RolePermissionRel WHERE RoleId = @roleId
	DELETE FROM Security_UserRoleRelation WHERE RoleId = @roleId
	DELETE FROM Security_Role WHERE RoleId = @roleId
END
GO

IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'Maven')
BEGIN
	DECLARE @roleId INT = (SELECT RoleId FROM Security_Role WHERE Name = 'Maven')
	DELETE FROM Security_RolePermissionRel WHERE RoleId = @roleId
	DELETE FROM Security_UserRoleRelation WHERE RoleId = @roleId
	DELETE FROM Security_Role WHERE RoleId = @roleId
END
GO

IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'Patron')
BEGIN
	DECLARE @roleId INT = (SELECT RoleId FROM Security_Role WHERE Name = 'Patron')
	DELETE FROM Security_RolePermissionRel WHERE RoleId = @roleId
	DELETE FROM Security_UserRoleRelation WHERE RoleId = @roleId
	DELETE FROM Security_Role WHERE RoleId = @roleId
END
GO

IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'Auditor')
BEGIN
	DECLARE @roleId INT = (SELECT RoleId FROM Security_Role WHERE Name = 'Auditor')
	DELETE FROM Security_RolePermissionRel WHERE RoleId = @roleId
	DELETE FROM Security_UserRoleRelation WHERE RoleId = @roleId
	DELETE FROM Security_Role WHERE RoleId = @roleId
END
GO

IF EXISTS (SELECT * FROM Security_Role WHERE Name = 'FormsDesigner')
BEGIN
	DECLARE @roleId INT = (SELECT RoleId FROM Security_Role WHERE Name = 'FormsDesigner')
	DELETE FROM Security_RolePermissionRel WHERE RoleId = @roleId
	DELETE FROM Security_UserRoleRelation WHERE RoleId = @roleId
	DELETE FROM Security_Role WHERE RoleId = @roleId
END
GO
--adding sales role
IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'Sales')
BEGIN 
	INSERT INTO dbo.Security_Role(Name, Description) VALUES('Sales', 'Sales person')
	DECLARE @roleId INT = SCOPE_IDENTITY()
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) 
	SELECT @roleId, p.Id FROM Security_Permission p
	WHERE p.Name IN ('CRM','AddDebitCard','EmailConfirmationButton','TestUser','SendingMessagesToClients','OpenBug','AddBankAccount','ChangeBankAccount','CheckBankAccount','HideBrokerClientDetails','SuspendBtn','ReturnBtn')
END 
GO
--adding broker sales role
IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'BrokerSales')
BEGIN 
	INSERT INTO dbo.Security_Role(Name, Description) VALUES('BrokerSales', 'Broker Sales person')
	DECLARE @roleId INT = SCOPE_IDENTITY()
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) 
	SELECT @roleId, p.Id FROM Security_Permission p
	WHERE p.Name IN ('CRM','AddDebitCard','EmailConfirmationButton','TestUser','SendingMessagesToClients','OpenBug','AddBankAccount','ChangeBankAccount','CheckBankAccount','SuspendBtn','ReturnBtn')
END 
GO
--adding junior uw role
IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name = 'JuniorUnderwriter')
BEGIN 
	INSERT INTO dbo.Security_Role(Name, Description) VALUES('JuniorUnderwriter', 'Junior Underwriter')
END 
GO


--roles
DECLARE @manager INT = (SELECT RoleId FROM Security_Role WHERE Name = 'manager')
DECLARE @uw INT = (SELECT RoleId FROM Security_Role WHERE Name = 'Underwriter')
DECLARE @juw INT = (SELECT RoleId FROM Security_Role WHERE Name = 'JuniorUnderwriter')
DECLARE @collector INT = (SELECT RoleId FROM Security_Role WHERE Name = 'Collector')

--permisions
DECLARE @rollover INT = (SELECT Id FROM Security_Permission WHERE Name = 'Rollover')
DECLARE @makePayment INT = (SELECT Id FROM Security_Permission WHERE Name = 'MakePayment')
DECLARE @loanOptions INT = (SELECT Id FROM Security_Permission WHERE Name = 'LoanOptions')
DECLARE @avoidAutomaticDecision INT = (SELECT Id FROM Security_Permission WHERE Name = 'AvoidAutomaticDecision')
DECLARE @suspend INT = (SELECT Id FROM Security_Permission WHERE Name = 'SuspendBtn')
DECLARE @discountPlan INT = (SELECT Id FROM Security_Permission WHERE Name = 'DiscountPlan')
DECLARE @disableShop INT = (SELECT Id FROM Security_Permission WHERE Name = 'DisableShop')
DECLARE @returnBtn INT = (SELECT Id FROM Security_Permission WHERE Name = 'ReturnBtn')

-- manager,uw,collector - rollover
IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@manager AND PermissionId=@rollover)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@manager, @rollover) 


IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@uw AND PermissionId=@rollover)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@uw, @rollover) 


IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@collector AND PermissionId=@rollover)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@collector, @rollover) 


-- manager,uw,collector - make payment
IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@manager AND PermissionId=@makePayment)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@manager, @makePayment) 


IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@uw AND PermissionId=@makePayment)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@uw, @makePayment) 


IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@collector AND PermissionId=@makePayment)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@collector, @makePayment) 


-- manager,uw,collector - loan options
IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@manager AND PermissionId=@loanOptions)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@manager, @loanOptions) 


IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@uw AND PermissionId=@loanOptions)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@uw, @loanOptions) 


IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@collector AND PermissionId=@loanOptions)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@collector, @loanOptions) 


-- manager,uw- avoidAutomaticDecision
IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@manager AND PermissionId=@avoidAutomaticDecision)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@manager, @avoidAutomaticDecision) 


IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@uw AND PermissionId=@avoidAutomaticDecision)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@uw, @avoidAutomaticDecision) 


--manager, uw, juw - suspend
IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@manager AND PermissionId=@suspend)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@manager, @suspend) 


IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@uw AND PermissionId=@suspend)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@uw, @suspend) 


IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@juw AND PermissionId=@suspend)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@juw, @suspend) 


--manager, uw, juw - return btn
IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@manager AND PermissionId=@returnBtn)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@manager, @returnBtn) 


IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@uw AND PermissionId=@returnBtn)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@uw, @returnBtn) 


IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@juw AND PermissionId=@returnBtn)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@juw, @returnBtn) 


--manager, uw - discount plan
IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@manager AND PermissionId=@discountPlan)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@manager, @discountPlan) 


IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@uw AND PermissionId=@discountPlan)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@uw, @discountPlan) 


--manager, uw - disable shop
IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@manager AND PermissionId=@disableShop)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@manager, @disableShop) 


IF NOT EXISTS (SELECT * FROM Security_RolePermissionRel WHERE RoleId=@uw AND PermissionId=@disableShop)
	INSERT INTO Security_RolePermissionRel (RoleId, PermissionId) VALUES (@uw, @disableShop) 


-- collector - 'CRM','AddDebitCard','EmailConfirmationButton','TestUser','SendingMessagesToClients','OpenBug','AddBankAccount','ChangeBankAccount','CheckBankAccount'
INSERT INTO Security_RolePermissionRel (RoleId, PermissionId)
SELECT @collector, p.Id  FROM Security_Permission p WHERE 
p.Name IN ('CRM','AddDebitCard','EmailConfirmationButton','TestUser','SendingMessagesToClients','OpenBug','AddBankAccount','ChangeBankAccount','CheckBankAccount')
AND p.Id NOT IN (SELECT rp.PermissionId FROM Security_RolePermissionRel rp WHERE RoleId=@collector)


