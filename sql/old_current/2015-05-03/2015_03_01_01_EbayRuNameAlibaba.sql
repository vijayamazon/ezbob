DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL 
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayRuNameAlibaba')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('EbayRuNameAlibaba', 'test-test8f473-e17f--zgehhnyon', 'Ebay ru name for alibaba', NULL)
	END
END

IF @Environment = 'QA' 
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayRuNameAlibaba')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('EbayRuNameAlibaba', 'Ezbob-Ezbob342e-6544--xdbkd', 'Ebay ru name for alibaba', NULL)
	END
END

IF @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayRuNameAlibaba')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('EbayRuNameAlibaba', 'Scorto-Scortoe95-67d5--luyco', 'Ebay ru name for alibaba', NULL)
	END
END



IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayRuNameAlibaba')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('EbayRuNameAlibaba', 'test-test8c677-69fb--izvtjwre', 'Ebay ru name for alibaba', NULL)
	END
END
GO
