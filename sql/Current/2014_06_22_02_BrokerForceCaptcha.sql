IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'BrokerForceCaptcha')
	INSERT INTO ConfigurationVariables (Name, Value, Description)
		VALUES('BrokerForceCaptcha', '0', 'Boolean. Force captcha during sign up')
GO