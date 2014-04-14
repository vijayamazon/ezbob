IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='YodleeApplicationToken')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('YodleeApplicationToken', '1ece74e1ca9e4befbb1b64daba7c4a24', 'Yodlee applicationtoken')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='YodleeApplicationKey')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('YodleeApplicationKey', 'a458bdf184d34c0cab7ef7ffbb5f016b', 'Yodlee application key')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='YodleeAccountPoolSize')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('YodleeAccountPoolSize', '30', 'Yodlee account pool size')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='YodleeBridgetApplicationId')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('YodleeBridgetApplicationId', '10003200', 'Yodlee bridget application id')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='YodleeTncVersion')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('YodleeTncVersion', '2', 'Yodlee tnc version')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='YodleePassword')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('YodleePassword', '3dhjzb873ehiw$#*bb', 'Yodlee password')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='YodleeApplicationId')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('YodleeApplicationId', '58EEA306454D869DFF721D0D00B82D00', 'Yodlee application id')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='YodleeSoapServer')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('YodleeSoapServer', 'https://sdkint11.yodlee.com/yodsoap/services', 'Yodlee soap server')
END
GO

DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL OR @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='YodleeCobrandId')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('YodleeCobrandId', '6910005960', 'Yodlee cobrand id')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='YodleeUsername')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('YodleeUsername', 'private-ezbob', 'Yodlee username')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='YodleeAddAccountURL')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('YodleeAddAccountURL', 'https://lawint.yodlee.com/apps/private-ezbob/addAccounts.pfmlaw.action', 'Yodlee add account url')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='YodleeEditAccountURL')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('YodleeEditAccountURL', 'https://lawint.yodlee.com/apps/private-ezbob/editSiteCredentials.pfmlaw.action', 'Yodlee edit account url')
	END
END
ELSE
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='YodleeCobrandId')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('YodleeCobrandId', '7110005960', 'Yodlee cobrand id')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='YodleeUsername')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('YodleeUsername', 'ezbob', 'Yodlee username')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='YodleeAddAccountURL')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('YodleeAddAccountURL', 'https://lawint.yodlee.com/apps/ezbob/addAccounts.pfmlaw.action', 'Yodlee add account url')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='YodleeEditAccountURL')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('YodleeEditAccountURL', 'https://lawint.yodlee.com/apps/ezbob/editSiteCredentials.pfmlaw.action', 'Yodlee edit account url')
	END
END
GO
