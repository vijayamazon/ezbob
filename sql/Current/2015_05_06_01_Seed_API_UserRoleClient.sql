
update ConfigurationVariables  set Value = 'Dev' where Name = 'Environment';

DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

select @Environment

-- USER
BEGIN
	IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [UserName]='elka') BEGIN
		INSERT INTO [dbo].[AspNetUsers] ([Id],[Email],[EmailConfirmed],[PasswordHash],[SecurityStamp],[PhoneNumber],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEndDateUtc],[LockoutEnabled],[AccessFailedCount],[UserName]) VALUES ('6a06814a-5afc-4d8a-ac3a-508936fb6fac', NULL, 0, 'AKmD0VoHvRQFNZ8Z+Vlnp0geJkM/pbBaM6aizqEcOOWrEAJ3lkDe2ktEzutd56hTug==','9a1ed198-96de-40b6-a487-10fa8b9eeb17', NULL,0,	0,	NULL,	0,	0,	'elka')
	END
	ELSE BEGIN	
		UPDATE [dbo].[AspNetUsers] set [Email] ='elinar@ezbob.com' WHERE [UserName] = 'elka';
	END
	
	IF (@Environment = 'Prod' OR @Environment = 'Dev' OR @Environment IS NULL)  BEGIN
		IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [UserName]='partherAppAlibaba') BEGIN		
			INSERT INTO [dbo].[AspNetUsers] ([Id],[Email],[EmailConfirmed],[PasswordHash],[SecurityStamp],[PhoneNumber],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEndDateUtc],[LockoutEnabled],[AccessFailedCount],[UserName]) VALUES ('b02fe9a2-636c-4aa9-80e4-ed3735dd3cdb', NULL, 0, 'ANo9AB92FIpBzf5Jv34e0oZVgnkNJ64RezgTEuiEi9/W1L9oiusXFIbmw1UkmMD40w==','03fa134c-2668-467b-9654-85098498edbb', NULL,0,	0,	NULL,	0,	0,	'partherAppAlibaba')
		END
		ELSE BEGIN
			UPDATE [dbo].[AspNetUsers] set [Email] ='elinar@ezbob.com' WHERE [UserName] = 'partherAppAlibaba';
		END
	END
	IF (@Environment = 'UAT' OR @Environment = 'Dev' OR @Environment IS NULL ) BEGIN
		IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [UserName]='partherAppAlibabaStaging')	BEGIN		
			INSERT INTO [dbo].[AspNetUsers] ([Id],[Email],[EmailConfirmed],[PasswordHash],[SecurityStamp],[PhoneNumber],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEndDateUtc],[LockoutEnabled],[AccessFailedCount],[UserName]) VALUES ('4839530c-7a0c-4e09-a9fe-d4e9595e2745', 'elinar@ezbob.com', 0, 'AHexOhqkXSp440Kx3c/HuJYMJbYZKcaQTYbhO00YoJ9vOn3kFcRGjK6NgbYF0/30xw==','33a86f1b-f615-4f00-b775-863b46f341f2', NULL,0,0,NULL,0,0,	'partherAppAlibabaStaging')
		END
		ELSE BEGIN
			UPDATE [dbo].[AspNetUsers] set [Email] ='elinar@ezbob.com'  WHERE [UserName] = 'partherAppAlibabaStaging';
		END		
	END		
END


-- CLIENTS
BEGIN
	IF NOT EXISTS (SELECT 1 FROM [Clients] WHERE Id='consoleApp') BEGIN
		INSERT INTO [dbo].[Clients] ([Id],[Secret],[Name],[ApplicationType],[Active],[RefreshTokenLifeTime],[AllowedOrigin]) VALUES  ('consoleApp',	'lCXDroz4HhR1EIx8qaz3C13z/quTXBkQ3Q5hj7Qx3aA=','Console Application',1,1,14400,'*')	
	END

	IF NOT EXISTS (SELECT 1 FROM [Clients] WHERE Id='ngAuthApp') BEGIN		
		INSERT INTO [dbo].[Clients] ([Id],[Secret],[Name],[ApplicationType],[Active],[RefreshTokenLifeTime],[AllowedOrigin]) VALUES  ('ngAuthApp',	'5YV7M1r981yoGhELyB84aC+KiYksxZf1OY3++C1CtRM=','Front-end Application',0,1,7200,'https://localhost:44302')
	END	
	
	IF (@Environment = 'Prod' OR @Environment = 'Dev' OR @Environment IS NULL)  BEGIN
		IF NOT EXISTS (SELECT 1 FROM [Clients] WHERE Id='pAliClient86f35Fd2896') BEGIN		
			INSERT INTO [dbo].[Clients] ([Id],[Secret],[Name],[ApplicationType],[Active],[RefreshTokenLifeTime],[AllowedOrigin]) VALUES  ('pAliClient86f35Fd2896',	'gcpbR2+gjPSwFAuXllnELIpM/ThkGtBNnNgf+0R/z8Y=','Alibaba partner client application',0,1,7200,'https://localhost:44302')
		END	

		IF NOT EXISTS (SELECT 1 FROM [Clients] WHERE Id='pAliServer7c60C021e70B') BEGIN		
			INSERT INTO [dbo].[Clients] ([Id],[Secret],[Name],[ApplicationType],[Active],[RefreshTokenLifeTime],[AllowedOrigin]) VALUES  ('pAliServer7c60C021e70B',	'rM7jVn+pwmiEuKLCAjHaQzt+mmrKFBZqPH4nxHcwjOg=','Alibaba partner server application',1,1,14400,'*')
		END
	END
	
	IF (@Environment = 'UAT' OR @Environment = 'Dev' OR @Environment IS NULL ) BEGIN
		IF NOT EXISTS (SELECT 1 FROM [Clients] WHERE Id='pAliClientStaging8256') BEGIN		
			INSERT INTO [dbo].[Clients] ([Id],[Secret],[Name],[ApplicationType],[Active],[RefreshTokenLifeTime],[AllowedOrigin]) VALUES  
			('pAliClientStaging8256', 'sh3D6es1Yh8LOpiI8eIgk/efItniKLvr5UfYuM/Qq78=', 'Staging: Alibaba client application', 0, 1, 7200, 'https://localhost:44302')
		END	
	
		IF NOT EXISTS (SELECT 1 FROM [Clients] WHERE Id='pAliServerStaging8256') BEGIN		
			INSERT INTO [dbo].[Clients] ([Id],[Secret],[Name],[ApplicationType],[Active],[RefreshTokenLifeTime],[AllowedOrigin]) VALUES  ('pAliServerStaging8256',	'Hm/MELfd0ND1cHhZtwb1850pTczxDqX+hxfbm9Jvvyo=','Staging: Alibaba server application',1,1,14400,'*')
		END
	END
END

-- ROLES/USER_ROLES
BEGIN
	IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = 'Administrator') BEGIN
		INSERT INTO [dbo].[AspNetRoles] ([Id],[Name]) VALUES ('429f056c-52b4-4fb3-9415-6a637274771e', 'Administrator')	
	END	
	
	IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = 'PartnerAlibaba') BEGIN		
		INSERT INTO [dbo].[AspNetRoles] ([Id],[Name]) VALUES ('3a10f384-fd84-4b9e-ac6e-e4a4d1237582', 'PartnerAlibaba')	
	END
	
	IF NOT EXISTS (SELECT * from [dbo].[AspNetUserRoles] WHERE [UserId] = (select Id FROM [dbo].[AspNetUsers] WHERE UserName = 'elka') and [RoleId] = (select Id FROM [dbo].[AspNetRoles] where Name = 'Administrator')) 
	BEGIN
		INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) 
		SELECT (SELECT Id from [dbo].[AspNetUsers] WHERE UserName = 'elka') as UserId, 
			Id FROM [dbo].[AspNetRoles] WHERE Name = 'Administrator' -- role
	END
	
	IF (@Environment = 'Prod' OR @Environment = 'Dev' OR @Environment IS NULL) BEGIN
		IF NOT EXISTS (SELECT * from [dbo].[AspNetUserRoles] WHERE [UserId] = (select Id FROM [dbo].[AspNetUsers] WHERE UserName = 'partherAppAlibaba') and [RoleId] = (select Id FROM [dbo].[AspNetRoles] where Name = 'PartnerAlibaba')) 
		BEGIN
			INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) 
			SELECT (SELECT Id from [dbo].[AspNetUsers] WHERE UserName = 'partherAppAlibaba') as UserId, 
				Id FROM [dbo].[AspNetRoles] WHERE Name = 'PartnerAlibaba' -- role
		END	
				
		-- remove staging data from production DB
		IF @Environment = 'Prod' BEGIN
			-- remove production Alibaba user and his roles from Staging
			IF EXISTS (SELECT * from [dbo].[AspNetUserRoles] WHERE [UserId] = (select Id FROM [dbo].[AspNetUsers] WHERE UserName = 'partherAppAlibabaStaging') and [RoleId] = (select Id FROM [dbo].[AspNetRoles] WHERE Name = 'PartnerAlibaba')) BEGIN
				DELETE from [AspNetUserRoles] WHERE [UserId]=(select Id FROM [dbo].[AspNetUsers] WHERE UserName = 'partherAppAlibabaStaging' );
			END	
			-- remove staging user from Prod
			IF EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [UserName]='partherAppAlibabaStaging') BEGIN
				DELETE from [AspNetUsers] WHERE [UserName]='partherAppAlibabaStaging';
			END	
			-- remove staging clients from Prod 
			IF EXISTS (SELECT 1 FROM [Clients] WHERE Id='pAliClientStaging8256') BEGIN	
				DELETE FROM [Clients] WHERE Id='pAliClientStaging8256';
			END
			IF EXISTS (SELECT 1 FROM [Clients] WHERE Id='pAliServerStaging8256') BEGIN	
				DELETE FROM [Clients] WHERE Id='pAliServerStaging8256';
			END
		END
	END
	
	IF (@Environment = 'UAT' OR @Environment = 'Dev' OR @Environment IS NULL) BEGIN
		IF NOT EXISTS (SELECT * from [dbo].[AspNetUserRoles] WHERE [UserId] = (select Id FROM [dbo].[AspNetUsers] WHERE UserName = 'partherAppAlibabaStaging') and [RoleId] = (select Id FROM [dbo].[AspNetRoles] r WHERE Name = 'PartnerAlibaba')) 
		BEGIN
			INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) 	
			SELECT 
				(SELECT Id FROM [dbo].[AspNetUsers] WHERE UserName = 'partherAppAlibabaStaging') as UserId, 
				Id FROM [dbo].[AspNetRoles] WHERE Name = 'PartnerAlibaba' -- role
		END	
	END
	
	-- remove production data from staging DB
	IF @Environment = 'UAT' BEGIN
		-- remove production Alibaba user and his roles from Staging
		IF EXISTS (SELECT * from [dbo].[AspNetUserRoles] WHERE [UserId] = (select Id FROM [dbo].[AspNetUsers] WHERE UserName = 'partherAppAlibaba') and [RoleId] = (select Id FROM [dbo].[AspNetRoles] WHERE Name = 'PartnerAlibaba')) BEGIN
			DELETE from [AspNetUserRoles] WHERE [UserId]=(select Id FROM [dbo].[AspNetUsers] WHERE UserName = 'partherAppAlibaba' );
		END	
		-- remove production user from Staging
		IF EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [UserName]='partherAppAlibaba') BEGIN
			DELETE from [AspNetUsers] WHERE [UserName]='partherAppAlibaba';
		END	
		-- remove production clients from Staging 
		IF EXISTS (SELECT 1 FROM [Clients] WHERE Id='pAliClient86f35Fd2896') BEGIN	
			DELETE FROM [Clients] WHERE Id='pAliClient86f35Fd2896';
		END
		IF EXISTS (SELECT 1 FROM [Clients] WHERE Id='pAliServer7c60C021e70B') BEGIN	
			DELETE FROM [Clients] WHERE Id='pAliServer7c60C021e70B';
		END
	END				
END
GO