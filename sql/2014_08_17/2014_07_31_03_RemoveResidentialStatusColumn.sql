IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ResidentialStatus' and Object_ID = Object_ID(N'Customer'))
BEGIN
	DECLARE @Statement NVARCHAR(MAX)
	SET @Statement = '
	DECLARE @StatusId INT
	
	SELECT @StatusId = Id FROM CustomerPropertyStatuses WHERE Description = ''Home owner''	
	UPDATE Customer SET PropertyStatusId = @StatusId WHERE ResidentialStatus = ''Home owner''
	
	SELECT @StatusId = Id FROM CustomerPropertyStatuses WHERE Description = ''Renting''	
	UPDATE Customer SET PropertyStatusId = @StatusId WHERE ResidentialStatus = ''Renting''
	
	SELECT @StatusId = Id FROM CustomerPropertyStatuses WHERE Description = ''Living with Parents''
	UPDATE Customer SET PropertyStatusId = @StatusId WHERE ResidentialStatus = ''Living with Parents''
	
	ALTER TABLE Customer DROP COLUMN ResidentialStatus
	'
	
	EXEC (@Statement)
END 
GO
