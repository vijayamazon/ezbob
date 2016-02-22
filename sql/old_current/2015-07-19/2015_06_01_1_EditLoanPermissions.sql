-- collector's new roles
IF NOT EXISTS( SELECT [RoleId] FROM [dbo].[Security_Role] WHERE [Name] = 'CollectorSenior' ) BEGIN 
	INSERT INTO  [dbo].[Security_Role] ([Name]) VALUES('CollectorSenior');
END;
IF NOT EXISTS( SELECT [RoleId] FROM [dbo].[Security_Role] WHERE [Name] = 'CollectorManager' ) BEGIN 
	INSERT INTO  [dbo].[Security_Role] ([Name]) VALUES('CollectorManager');
END; 

-- permissions
IF NOT EXISTS( SELECT [Id] FROM [dbo].[Security_Permission] WHERE [Name] = 'RescheduleOutOfLoanButton' ) BEGIN 
	declare @lastid int;
	SET @lastid = (SELECT Max(Id) as i FROM [dbo].[Security_Permission]);	
	INSERT INTO  [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES((@lastid +1),'RescheduleOutOfLoanButton', 'Rescheduling "Outside Loan Payment Arrangement"');
END;


-- values for permission
IF NOT EXISTS( SELECT [Id] FROM [dbo].[ConfigurationVariables] WHERE [Name] = 'RescheduleOutOfLoan_Collector_MAX_INTERVALS' ) BEGIN 	
	INSERT INTO [dbo].[ConfigurationVariables] ( [Name], [Value], [Description]) VALUES('RescheduleOutOfLoan_Collector_MAX_INTERVALS', '18', 'Maximal repayment intervals allowed for Collector role during Rescheduling "Outside Loan Payment Arrangement"');
END;

IF NOT EXISTS( SELECT [Id] FROM [dbo].[ConfigurationVariables] WHERE [Name] = 'RescheduleOutOfLoan_Collector_MAX_AMOUNT' ) BEGIN 	
	INSERT INTO [dbo].[ConfigurationVariables] ( [Name], [Value], [Description]) VALUES('RescheduleOutOfLoan_Collector_MAX_AMOUNT', '30000', 'Maximal amount allowed for Collector role during Rescheduling "Outside Loan Payment Arrangement"');
END;

IF NOT EXISTS( SELECT [Id] FROM [dbo].[ConfigurationVariables] WHERE [Name] = 'RescheduleOutOfLoan_CollectorSenior_MAX_INTERVALS' ) BEGIN 	
	INSERT INTO [dbo].[ConfigurationVariables] ( [Name], [Value], [Description]) VALUES('RescheduleOutOfLoan_CollectorSenior_MAX_INTERVALS', '48', 'Maximal repayment intervals allowed for CollectorSenior role during Rescheduling "Outside Loan Payment Arrangement"');
END;

IF NOT EXISTS( SELECT [Id] FROM [dbo].[ConfigurationVariables] WHERE [Name] = 'RescheduleOutOfLoan_CollectorSenior_MAX_AMOUNT' ) BEGIN 	
	INSERT INTO [dbo].[ConfigurationVariables] ( [Name], [Value], [Description]) VALUES('RescheduleOutOfLoan_CollectorSenior_MAX_AMOUNT', '100000', 'Maximal amount allowed for Collector role during Rescheduling "Outside Loan Payment Arrangement"');
END;

declare  @RoleID int;
declare  @PermissionID int;
declare  @UserID int;


set @RoleID = (select [RoleId] from [dbo].[Security_Role] where [Name] = 'Collector');
set @PermissionID = (select [Id] from [dbo].[Security_Permission] where  [Name] ='RescheduleOutOfLoanButton');

-- permissions to roles
IF NOT EXISTS( SELECT [RoleId], [PermissionId]  FROM [dbo].[Security_RolePermissionRel] WHERE [RoleId]  =@RoleID and [PermissionId] = @PermissionID) BEGIN 	
	INSERT INTO [dbo].[Security_RolePermissionRel] ([RoleId],[PermissionId]) values (@RoleID, @PermissionID);
END;

set @RoleID = (select [RoleId] from [dbo].[Security_Role] where [Name] = 'CollectorSenior');
IF NOT EXISTS( SELECT [RoleId], [PermissionId]  FROM [dbo].[Security_RolePermissionRel] WHERE [RoleId]  =@RoleID and [PermissionId] = @PermissionID) BEGIN 	
	INSERT INTO [dbo].[Security_RolePermissionRel] ([RoleId],[PermissionId]) values (@RoleID, @PermissionID);
END;

set @RoleID = (select [RoleId] from [dbo].[Security_Role] where [Name] = 'CollectorManager');
IF NOT EXISTS( SELECT [RoleId], [PermissionId]  FROM [dbo].[Security_RolePermissionRel] WHERE [RoleId]  =@RoleID and [PermissionId] = @PermissionID) BEGIN 	
	INSERT INTO [dbo].[Security_RolePermissionRel] ([RoleId],[PermissionId]) values (@RoleID, @PermissionID);
END;

-- BE SURE USERNAME ARE CORRECT !!!!!!!!!!!!!!!

-- set @RoleID = (select [RoleId] from [dbo].[Security_Role] where [Name] = 'Collector');
-- --select @RoleID;
-- set @UserID = (select [UserId] from [dbo].[Security_User] where [UserName] = 'emma')
-- --select @UserID;
-- --SELECT [UserId], [RoleId]  FROM [dbo].[Security_UserRoleRelation] WHERE [UserId] = @UserID and [RoleId] = @RoleID
-- IF @RoleID IS NOT NULL AND @UserID IS NOT NULL BEGIN 
	-- IF NOT EXISTS( SELECT [UserId], [RoleId] FROM [dbo].[Security_UserRoleRelation] WHERE [UserId] = @UserID and [RoleId] = @RoleID) BEGIN
		-- INSERT INTO [dbo].[Security_UserRoleRelation] ([UserId],[RoleId]) values (@UserID, @RoleID);
	-- END;
-- END;

-- set @RoleID = (select [RoleId] from [dbo].[Security_Role] where [Name] = 'CollectorSenior');
-- set @UserID = (select [UserId] from [dbo].[Security_User] where [UserName] = 'russellb')
-- IF @RoleID IS NOT NULL AND @UserID IS NOT NULL BEGIN 
	-- IF NOT EXISTS( SELECT [UserId], [RoleId] FROM [dbo].[Security_UserRoleRelation] WHERE [UserId] = @UserID and [RoleId] = @RoleID) BEGIN
		-- INSERT INTO [dbo].[Security_UserRoleRelation] ([UserId],[RoleId]) values (@UserID, @RoleID);
	-- END;
-- END;


-- set @RoleID = (select [RoleId] from [dbo].[Security_Role] where [Name] = 'CollectorManager');
-- set @UserID = (select [UserId] from [dbo].[Security_User] where [UserName] = 'vitasd')
-- IF @RoleID IS NOT NULL AND @UserID IS NOT NULL BEGIN 
	-- IF NOT EXISTS( SELECT [UserId], [RoleId] FROM [dbo].[Security_UserRoleRelation] WHERE [UserId] = @UserID and [RoleId] = @RoleID) BEGIN
		-- INSERT INTO [dbo].[Security_UserRoleRelation] ([UserId],[RoleId]) values (@UserID, @RoleID);
	-- END;
-- END;


-- set @RoleID = (select [RoleId] from [dbo].[Security_Role] where [Name] = 'CollectorManager');
-- set @UserID = (select [UserId] from [dbo].[Security_User] where [UserName] =  '%tomerg%');
-- IF @RoleID IS NOT NULL AND @UserID IS NOT NULL BEGIN 
	-- IF NOT EXISTS( SELECT [UserId], [RoleId] FROM [dbo].[Security_UserRoleRelation] WHERE [UserId] = @UserID and [RoleId] = @RoleID) BEGIN
		-- INSERT INTO [dbo].[Security_UserRoleRelation] ([UserId],[RoleId]) values (@UserID, @RoleID);
	-- END;
-- END;
