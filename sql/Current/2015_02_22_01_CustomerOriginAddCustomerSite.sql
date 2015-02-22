IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='CustomerSite' AND id=object_id('CustomerOrigin'))
BEGIN
	ALTER TABLE CustomerOrigin ADD CustomerSite NVARCHAR(255)
END 
GO

DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL 
BEGIN
	UPDATE CustomerOrigin SET CustomerSite = 'https://test.ezbob.com' WHERE Name = 'ezbob'
	UPDATE CustomerOrigin SET CustomerSite = 'https://test.everline.com' WHERE Name = 'everline'
END

IF @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	UPDATE CustomerOrigin SET CustomerSite = 'https://test.ezbob.com' WHERE Name = 'ezbob'
	UPDATE CustomerOrigin SET CustomerSite = 'https://test.everline.com' WHERE Name = 'everline'
END

IF @Environment = 'Prod'
BEGIN
	UPDATE CustomerOrigin SET CustomerSite = 'https://app.ezbob.com' WHERE Name = 'ezbob'
	UPDATE CustomerOrigin SET CustomerSite = 'https://app.everline.com' WHERE Name = 'everline'
END
GO
