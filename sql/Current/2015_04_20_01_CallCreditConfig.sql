DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'


IF @Environment = 'UAT' OR @Environment = 'Dev' OR @Environment = 'QA' or @Environment IS NULL
BEGIN

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CallCreditUrl')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('CallCreditUrl', 'https://ct.callcreditsecure.co.uk/Services/BSB/CRBSB7.asmx', 'CallCredit api url', NULL)
	END

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CallCreditUserCompany')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('CallCreditUserCompany', 'Ezbob SR CTEST', 'User`s company at CallCredit', NULL)
	END

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CallCreditUserName')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('CallCreditUserName', 'Ezbob SR API CTEST', 'Username at CallCredit', NULL)
	END

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CallCreditPassword')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('CallCreditPassword', '7UM9AXH2', 'CallCredit Password', NULL)
	END
END



IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CallCreditUrl')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('CallCreditUrl', 'https://www.callcreditsecure.co.uk/Services/BSB/CRBSB7.asmx', 'CallCredit api url', NULL)
	END

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CallCreditUserCompany')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('CallCreditUserCompany', '', 'User`s company at CallCredit', NULL)
	END

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CallCreditUserName')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('CallCreditUserName', '', 'Username at CallCredit', NULL)
	END

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CallCreditPassword')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
		VALUES('CallCreditPassword', '7UM9AXH2', 'CallCredit Password', NULL)
	END
END
GO

