DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL 
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayRuNameEverline')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('EbayRuNameEverline', 'test-test8f473-e17f--kzskzb', 'Ebay ru name for everline', NULL)
	END
END

IF @Environment = 'QA' 
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayRuNameEverline')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('EbayRuNameEverline', 'Ezbob-Ezbob342e-6544--arxujwkun', 'Ebay ru name for everline', NULL)
	END
END

IF @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayRuNameEverline')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('EbayRuNameEverline', '', 'Ebay ru name for everline', NULL)
	END
END



IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayRuNameEverline')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('EbayRuNameEverline', '', 'Ebay ru name for everline', NULL)
	END
END
GO
