

BEGIN
	IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = 'Administrator')
	BEGIN
		INSERT INTO [dbo].[AspNetRoles] ([Id],[Name]) VALUES ('429f056c-52b4-4fb3-9415-6a637274771e', 'Administrator')	
	END	
	
	IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = 'PartnerAlibaba')
	BEGIN		
		INSERT INTO [dbo].[AspNetRoles] ([Id],[Name]) VALUES ('3a10f384-fd84-4b9e-ac6e-e4a4d1237582', 'PartnerAlibaba')	
	END	
	
	IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = 'b02fe9a2-636c-4aa9-80e4-ed3735dd3cdb')
	BEGIN
		INSERT INTO [dbo].[AspNetUserRoles] ([UserId],[RoleId]) VALUES ('b02fe9a2-636c-4aa9-80e4-ed3735dd3cdb', '3a10f384-fd84-4b9e-ac6e-e4a4d1237582')
	END		
			
END
GO