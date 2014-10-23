DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationUrl')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationUrl', 'https://mpsnare.iesnare.com/script/snaregw.js', 'iovation prod URL')
	END
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationSubscriberCode')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationSubscriberCode', '', 'an iovation supplied value identifying your company/site')
	END
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationAccountCode')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationAccountCode', '', 'a unique id, you choose, that represents the account in your system. Account codes can be UTF-8 up to 80 bytes.')
	END
END
ELSE	
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationUrl')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationUrl', 'https://ci-mpsnare.iovation.com/script/snaregw.js', 'iovation test URL')
	END
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationSubscriberCode')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationSubscriberCode', '', 'an iovation supplied value identifying your company/site')
	END
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationAccountCode')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationAccountCode', '', 'a unique id, you choose, that represents the account in your system. Account codes can be UTF-8 up to 80 bytes.')
	END
END
GO
