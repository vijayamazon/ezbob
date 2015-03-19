
BEGIN
	IF NOT EXISTS (SELECT 1 FROM [Clients] WHERE Id='consoleApp')
	BEGIN
		INSERT INTO [dbo].[Clients] ([Id],[Secret],[Name],[ApplicationType],[Active],[RefreshTokenLifeTime],[AllowedOrigin]) VALUES  ('consoleApp',	'lCXDroz4HhR1EIx8qaz3C13z/quTXBkQ3Q5hj7Qx3aA=','Console Application',1,1,14400,'*')	END	
	END

	IF NOT EXISTS (SELECT 1 FROM [Clients] WHERE Id='ngAuthApp')
	BEGIN		
		INSERT INTO [dbo].[Clients] ([Id],[Secret],[Name],[ApplicationType],[Active],[RefreshTokenLifeTime],[AllowedOrigin]) VALUES  ('ngAuthApp',	'5YV7M1r981yoGhELyB84aC+KiYksxZf1OY3++C1CtRM=','Front-end Application',0,1,7200,'https://localhost:44302')
	END	
	
	IF NOT EXISTS (SELECT 1 FROM [Clients] WHERE Id='pAliClient86f35Fd2896')
	BEGIN		
		INSERT INTO [dbo].[Clients] ([Id],[Secret],[Name],[ApplicationType],[Active],[RefreshTokenLifeTime],[AllowedOrigin]) VALUES  ('pAliClient86f35Fd2896',	'gcpbR2+gjPSwFAuXllnELIpM/ThkGtBNnNgf+0R/z8Y=','Alibaba partner client application',0,1,7200,'https://localhost:44302')
	END	

	IF NOT EXISTS (SELECT 1 FROM [Clients] WHERE Id='pAliServer7c60C021e70B')
	BEGIN		
		INSERT INTO [dbo].[Clients] ([Id],[Secret],[Name],[ApplicationType],[Active],[RefreshTokenLifeTime],[AllowedOrigin]) VALUES  ('pAliServer7c60C021e70B',	'rM7jVn+pwmiEuKLCAjHaQzt+mmrKFBZqPH4nxHcwjOg=','Alibaba partner server application',1,1,14400,'*')
	END	
