DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL 
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='GoCardlessAppId')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('GoCardlessAppId', 'MP9EJ3MQH163M34V2D7SS567DV8G44BBSBF3QBFVJP0R736198P2G7E88N38B2RE', 'GoCardless App identifier')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='GoCardlessAppSecret')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('GoCardlessAppSecret', '0E866XN7Z33X5N4WRK5668DRESFV9WFP8DZMVW6YW0FFK48XM58KH9SFG7RBGVDT', 'GoCardless App secret')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='GoCardlessAccessToken')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('GoCardlessAccessToken', '3MMVNZERVE7X8FN8AGYZM3EBANVTD3NZENEVPN62BAVHY3FCEVQJ9JA6RZEDGTEF', 'GoCardless Merchant access token')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='GoCardlessMerchantId')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('GoCardlessMerchantId', '0NP016PZPT', 'GoCardless Merchant id')
	END
	
END

IF @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='GoCardlessAppId')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('GoCardlessAppId', '', 'GoCardless App identifier')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='GoCardlessAppSecret')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('GoCardlessAppSecret', '', 'GoCardless App secret')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='GoCardlessAccessToken')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('GoCardlessAccessToken', '', 'GoCardless Merchant access token')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='GoCardlessMerchantId')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('GoCardlessMerchantId', '', 'GoCardless Merchant id')
	END
END

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='GoCardlessAppId')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('GoCardlessAppId', 'QYBS6YTV79F12DKS6V12M0E4B8Y33N5DYN8HDZT4PN6M6SZ29HAJ1491FQ6RZ92V', 'GoCardless App identifier')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='GoCardlessAppSecret')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('GoCardlessAppSecret', '5R0HRQ2JN068NTWAYEZ10SR6XBF51Y4G8K44QQMGDFTPW7F868QWSSRS568PR20V', 'GoCardless App secret')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='GoCardlessAccessToken')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('GoCardlessAccessToken', 'DYXAHEEGG7ZEGH4X4FTN8QG99R6GZD5H0CZQYPGX6S0RDBQSJNCNDAM9M0MRXECQ', 'GoCardless Merchant access token')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='GoCardlessMerchantId')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('GoCardlessMerchantId', '0NP016PZPT', 'GoCardless Merchant id')
	END
END
GO