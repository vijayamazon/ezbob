IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'BrokerForceCaptcha')
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted)
		VALUES('BrokerForceCaptcha', '0', 'Boolean. Force captcha during sign up', 0)
GO