IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ResidentialStatus' and Object_ID = Object_ID(N'Customer'))
BEGIN
	DECLARE @Statement NVARCHAR(MAX)
	SET @Statement = '
	UPDATE Customer SET PropertyStatusId = 2 WHERE ResidentialStatus = ''Home owner''
	UPDATE Customer SET PropertyStatusId = 5 WHERE ResidentialStatus = ''Renting''
	UPDATE Customer SET PropertyStatusId = 7 WHERE ResidentialStatus = ''Living with Parents''
	ALTER TABLE Customer DROP COLUMN ResidentialStatus
	'
	
	EXEC (@Statement)
END 
GO
