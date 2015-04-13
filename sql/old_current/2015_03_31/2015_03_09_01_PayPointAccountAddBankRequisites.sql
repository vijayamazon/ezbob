IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='AccName' AND id=object_id('PayPointAccount'))
BEGIN
	ALTER TABLE PayPointAccount ADD AccName NVARCHAR(100)
END 
GO

IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='AccNumber' AND id=object_id('PayPointAccount'))
BEGIN
	ALTER TABLE PayPointAccount ADD AccNumber NVARCHAR(10)
END 
GO

IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='SortCode' AND id=object_id('PayPointAccount'))
BEGIN
	ALTER TABLE PayPointAccount ADD SortCode NVARCHAR(10)
END 
GO

DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL 
BEGIN
	UPDATE PayPointAccount SET AccName = 'Test', AccNumber = '12345678',SortCode = '123456' WHERE Mid = 'secpay'
END

IF @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	UPDATE PayPointAccount SET AccName = 'Test', AccNumber = '12345678',SortCode = '123456' WHERE Mid = 'secpay'
END

IF @Environment = 'Prod'
BEGIN
	UPDATE PayPointAccount SET AccName = 'OM Bridge', AccNumber = '20114966', SortCode = '621000' WHERE Mid = 'orange06'


	UPDATE PayPointAccount SET AccName = 'OM OAk', AccNumber = '20115636', SortCode = '621000' WHERE Mid = 'orange07'

	/*UPDATE PayPointAccount SET AccName = 'Ezbob Oak', AccNumber = '20110251', SortCode = '621000' WHERE Mid = 'orange08'*/
	
END
GO

