-- collector's new roles
IF NOT EXISTS( SELECT [RoleId] FROM [dbo].[Security_Role] WHERE [Name] = 'CollectorSenior' ) BEGIN 
	INSERT INTO  [dbo].[Security_Role] ([Name]) VALUES('CollectorSenior');
END;
IF NOT EXISTS( SELECT [RoleId] FROM [dbo].[Security_Role] WHERE [Name] = 'CollectorManager' ) BEGIN 
	INSERT INTO  [dbo].[Security_Role] ([Name]) VALUES('CollectorManager');
END; 


 