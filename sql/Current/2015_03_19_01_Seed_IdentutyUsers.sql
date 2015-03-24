
BEGIN
	IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [UserName]='elka')
	BEGIN
		INSERT INTO [dbo].[AspNetUsers] ([Id],[Email],[EmailConfirmed],[PasswordHash],[SecurityStamp],[PhoneNumber],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEndDateUtc],[LockoutEnabled],[AccessFailedCount],[UserName]) VALUES ('6a06814a-5afc-4d8a-ac3a-508936fb6fac', NULL, 0, 'AKmD0VoHvRQFNZ8Z+Vlnp0geJkM/pbBaM6aizqEcOOWrEAJ3lkDe2ktEzutd56hTug==','9a1ed198-96de-40b6-a487-10fa8b9eeb17', NULL,0,	0,	NULL,	0,	0,	'elka')
	END	
	
	IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [UserName]='partherAppAlibaba')
	BEGIN		
		INSERT INTO [dbo].[AspNetUsers] ([Id],[Email],[EmailConfirmed],[PasswordHash],[SecurityStamp],[PhoneNumber],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEndDateUtc],[LockoutEnabled],[AccessFailedCount],[UserName]) VALUES ('b02fe9a2-636c-4aa9-80e4-ed3735dd3cdb', NULL, 0, 'ANo9AB92FIpBzf5Jv34e0oZVgnkNJ64RezgTEuiEi9/W1L9oiusXFIbmw1UkmMD40w==','03fa134c-2668-467b-9654-85098498edbb', NULL,0,	0,	NULL,	0,	0,	'partherAppAlibaba')
	END		
END

GO